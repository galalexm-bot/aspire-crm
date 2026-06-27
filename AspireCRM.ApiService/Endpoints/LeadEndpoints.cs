using System.Security.Claims;
using AspireCRM.ApiService.Services;
using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;
using AspireCRM.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public record LeadStatusChangeRequest(string? Comment);
public record LeadBatchActivateRequest(long[] Ids);
public record MarkDuplicateRequest(long TargetLeadId, string? Comment);
public record DuplicateCandidate(long Id, string Name, string? Description, LeadStatus Status, double Similarity);
public record LeadConversionPreview(long LeadId, string LeadName, bool CanConvert, string[] AvailableTypes);
public record LeadConversionRequest(string? Comment, string? SaleName, double? SaleVolume, string? RelationshipTheme, string? RelationshipDescription, DateTime? RelationshipStartDate, DateTime? RelationshipEndDate);
public record LeadConversionResult(long LeadId, long ContractorId, long? SaleId, long? RelationshipId);
public record LeadBatchAssignRequest(long[] Ids, long ResponsibleId);
public record LeadBatchChangeSourceRequest(long[] Ids, long? SourceId);

public static class LeadEndpoints
{
    public static void MapLeadEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/leads");

        api.MapGet("/", async (IRepository<Lead> repo, int page = 1, int pageSize = 20, LeadStatus? status = null) =>
        {
            var filter = status.HasValue ? (System.Linq.Expressions.Expression<Func<Lead, bool>>)(l => l.Status == status.Value) : null;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
        }).RequireCategoryPermission(CategoryPermissionLevel.Read);

        api.MapGet("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            return lead is null ? Results.NotFound() : Results.Ok(lead);
        }).RequireCategoryPermission(CategoryPermissionLevel.Read);

        api.MapPost("/", async (Lead lead, IRepository<Lead> repo, HttpContext http, CategoryRuleService ruleService) =>
        {
            lead.CreationDate = DateTime.UtcNow;
            lead.ChangeDate = DateTime.UtcNow;
            lead.CreationAuthorId = GetUserId(http);
            lead.ChangeAuthorId = GetUserId(http);
            var created = await repo.AddAsync(lead);
            await ruleService.ApplyLeadRules(created);
            return Results.Created($"/api/leads/{created.Id}", created);
        }).RequireCategoryPermission(CategoryPermissionLevel.Write);

        api.MapPut("/{id:long}", async (long id, Lead lead, IRepository<Lead> repo, HttpContext http, CategoryRuleService ruleService) =>
        {
            if (id != lead.Id) return Results.BadRequest();
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);
            await ruleService.ApplyLeadRules(lead);
            return Results.NoContent();
        }).RequireCategoryPermission(CategoryPermissionLevel.Write);

        api.MapDelete("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            await repo.DeleteAsync(lead);
            return Results.NoContent();
        }).RequireCategoryPermission(CategoryPermissionLevel.Delete);

        api.MapPost("/{id:long}/begin", async (long id, HttpContext http, IRepository<Lead> repo, AuditService auditService) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            if (lead.Status == LeadStatus.InHand)
                return Results.BadRequest("Лид уже имеет статус \"В работе\"");

            var oldStatus = lead.Status.ToString();
            lead.Status = LeadStatus.InHand;
            lead.InHandDate = DateTime.UtcNow;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            await auditService.LogAsync("Lead", id, "Status", oldStatus, LeadStatus.InHand.ToString(), GetUserId(http));

            return Results.Ok(lead);
        });

        api.MapPost("/{id:long}/fail", async (long id, LeadStatusChangeRequest req, HttpContext http, IRepository<Lead> repo, IRepository<Comment> commentRepo, AuditService auditService) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            var oldStatus = lead.Status.ToString();
            lead.Status = LeadStatus.Unqualified;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            if (!string.IsNullOrWhiteSpace(req.Comment))
            {
                var comment = new Comment
                {
                    Text = req.Comment,
                    AuthorId = GetUserId(http),
                    CreationDate = DateTime.UtcNow
                };
                comment.Leads.Add(lead);
                await commentRepo.AddAsync(comment);
            }

            await auditService.LogAsync("Lead", id, "Status", oldStatus, LeadStatus.Unqualified.ToString(), GetUserId(http), req.Comment);

            return Results.Ok(lead);
        });

        api.MapPost("/{id:long}/conversation-not-start", async (long id, HttpContext http, IRepository<Lead> repo, AuditService auditService) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            if (lead.Status == LeadStatus.ConversationNotStart)
                return Results.BadRequest("Лид уже имеет статус \"Разговор не состоялся\"");

            var oldStatus = lead.Status.ToString();
            lead.Status = LeadStatus.ConversationNotStart;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            await auditService.LogAsync("Lead", id, "Status", oldStatus, LeadStatus.ConversationNotStart.ToString(), GetUserId(http));

            return Results.Ok(lead);
        });

        api.MapPost("/batch/activate", async (LeadBatchActivateRequest req, HttpContext http, IRepository<Lead> repo) =>
        {
            var errors = new List<string>();
            var successCount = 0;

            foreach (var id in req.Ids)
            {
                var lead = await repo.GetByIdAsync(id);
                if (lead is null) continue;

                if (lead.Status != LeadStatus.Unqualified && lead.Status != LeadStatus.Dublicate)
                {
                    errors.Add($"Лид #{id}: активация доступна только для статусов \"Неквалифицирован\" или \"Дубль\"");
                    continue;
                }

                lead.Status = LeadStatus.New;
                lead.DublicateLead = null;
                lead.DublicateContractor = null;
                lead.DublicateSale = null;
                lead.DublicateComment = null;
                lead.ChangeDate = DateTime.UtcNow;
                lead.ChangeAuthorId = GetUserId(http);
                await repo.UpdateAsync(lead);
                successCount++;
            }

            return Results.Ok(new { SuccessCount = successCount, Errors = errors });
        });

        api.MapPost("/batch/assign", async (LeadBatchAssignRequest req, HttpContext http, IRepository<Lead> repo) =>
        {
            var errors = new List<string>();
            var successCount = 0;

            foreach (var id in req.Ids)
            {
                var lead = await repo.GetByIdAsync(id);
                if (lead is null) continue;

                lead.ResponsibleId = req.ResponsibleId;
                lead.ChangeDate = DateTime.UtcNow;
                lead.ChangeAuthorId = GetUserId(http);
                await repo.UpdateAsync(lead);
                successCount++;
            }

            return Results.Ok(new { SuccessCount = successCount, Errors = errors });
        });

        api.MapPost("/batch/change-source", async (LeadBatchChangeSourceRequest req, HttpContext http, IRepository<Lead> repo) =>
        {
            var errors = new List<string>();
            var successCount = 0;

            foreach (var id in req.Ids)
            {
                var lead = await repo.GetByIdAsync(id);
                if (lead is null) continue;

                lead.SourceId = req.SourceId;
                lead.ChangeDate = DateTime.UtcNow;
                lead.ChangeAuthorId = GetUserId(http);
                await repo.UpdateAsync(lead);
                successCount++;
            }

            return Results.Ok(new { SuccessCount = successCount, Errors = errors });
        });

        api.MapGet("/check-duplicates", async (string name, IRepository<Lead> repo) =>
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
                return Results.Ok(new List<DuplicateCandidate>());

            var allLeads = await repo.FindAsync(l => !l.IsDeleted);
            var lowerName = name.ToLowerInvariant();
            var candidates = allLeads
                .Where(l => l.Name.ToLowerInvariant().Contains(lowerName) || lowerName.Contains(l.Name.ToLowerInvariant()))
                .Select(l => new DuplicateCandidate(
                    l.Id, l.Name, l.Description, l.Status,
                    CalculateSimilarity(name, l.Name)))
                .OrderByDescending(c => c.Similarity)
                .Take(10)
                .ToList();

            return Results.Ok(candidates);
        });

        api.MapGet("/{id:long}/duplicates", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            var allLeads = await repo.FindAsync(l => l.Id != id && !l.IsDeleted);
            var lowerName = lead.Name.ToLowerInvariant();
            var candidates = allLeads
                .Where(l => l.Name.ToLowerInvariant().Contains(lowerName) || lowerName.Contains(l.Name.ToLowerInvariant()))
                .Select(l => new DuplicateCandidate(
                    l.Id, l.Name, l.Description, l.Status,
                    CalculateSimilarity(lead.Name, l.Name)))
                .OrderByDescending(c => c.Similarity)
                .Take(10)
                .ToList();

            return Results.Ok(candidates);
        });

        api.MapPost("/{id:long}/mark-duplicate", async (long id, MarkDuplicateRequest req, HttpContext http, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            var target = await repo.GetByIdAsync(req.TargetLeadId);
            if (target is null) return Results.BadRequest("Целевой лид не найден");

            lead.Status = LeadStatus.Dublicate;
            lead.DublicateLead = target.Name;
            lead.DublicateComment = req.Comment;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            return Results.Ok(lead);
        });

        api.MapPost("/{id:long}/unmark-duplicate", async (long id, HttpContext http, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            lead.Status = LeadStatus.New;
            lead.DublicateLead = null;
            lead.DublicateContractor = null;
            lead.DublicateSale = null;
            lead.DublicateComment = null;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            return Results.Ok(lead);
        });

        api.MapGet("/{id:long}/convert/preview", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            var canConvert = lead.Status == LeadStatus.Qualified || lead.Status == LeadStatus.InHand;
            var types = canConvert
                ? new[] { "ContractorOnly", "ContractorAndSale", "ContractorAndRelationship" }
                : [];

            return Results.Ok(new LeadConversionPreview(lead.Id, lead.Name, canConvert, types));
        });

        api.MapPost("/{id:long}/convert", async (long id, LeadConversionRequest req, HttpContext http,
            IRepository<Lead> leadRepo, IRepository<Contractor> contractorRepo, IRepository<Sale> saleRepo,
            IRepository<Relationship> relRepo, IRepository<Contact> contactRepo,
            IRepository<Comment> commentRepo, AuditService auditService) =>
        {
            var lead = await leadRepo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

            if (lead.Status is not (LeadStatus.Qualified or LeadStatus.InHand))
                return Results.BadRequest("Конвертация доступна только для лидов в статусе \"Квалифицирован\" или \"В работе\"");

            var userId = GetUserId(http);
            var hasSale = !string.IsNullOrWhiteSpace(req.SaleName);
            var hasRelationship = !string.IsNullOrWhiteSpace(req.RelationshipTheme);

            var contractor = new Contractor
            {
                Name = lead.Name,
                Description = lead.Description,
                Site = lead.Site,
                ResponsibleId = lead.ResponsibleId,
                TenantId = lead.TenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                CreationDate = DateTime.UtcNow,
                CreationAuthorId = userId
            };

            if (lead.Address is not null)
            {
                contractor.LegalAddress = new Address
                {
                    Country = lead.Address.Country,
                    City = lead.Address.City,
                    Street = lead.Address.Street,
                    Building = lead.Address.Building,
                    Apartment = lead.Address.Apartment,
                    ZipCode = lead.Address.ZipCode,
                    FullAddress = lead.Address.FullAddress,
                    TenantId = lead.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };
            }

            foreach (var email in lead.Emails)
            {
                contractor.Emails.Add(new Email
                {
                    EmailAddress = email.EmailAddress,
                    Description = email.Description,
                    TenantId = lead.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                });
            }

            foreach (var phone in lead.Phones)
            {
                contractor.Phones.Add(new Phone
                {
                    PhoneNumber = phone.PhoneNumber,
                    Description = phone.Description,
                    TenantId = lead.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                });
            }

            foreach (var leadContact in lead.Contacts)
            {
                var contact = contactRepo.FindAsync(c => c.Id == leadContact.ContactId).Result.FirstOrDefault();
                if (contact is not null)
                {
                    contractor.Contacts.Add(contact);
                }
            }

            contractor = await contractorRepo.AddAsync(contractor);

            Sale? sale = null;
            if (hasSale)
            {
                sale = new Sale
                {
                    Name = req.SaleName!,
                    ContractorId = contractor.Id,
                    Contractor = contractor,
                    Description = lead.Description,
                    MarketingEffect = lead.MarketingEffect,
                    SaleStatus = SaleStatus.Active,
                    ResponsibleId = lead.ResponsibleId ?? userId,
                    AuthorId = userId,
                    StartDate = DateTime.UtcNow,
                    CreationDate = DateTime.UtcNow,
                    TenantId = lead.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId
                };
                if (req.SaleVolume.HasValue)
                    sale.SalesVolume = req.SaleVolume;
                sale = await saleRepo.AddAsync(sale);
            }

            Relationship? relationship = null;
            if (hasRelationship)
            {
                relationship = new Relationship
                {
                    Theme = req.RelationshipTheme!,
                    Description = req.RelationshipDescription ?? lead.Description,
                    ContractorId = contractor.Id,
                    StartDate = req.RelationshipStartDate ?? DateTime.UtcNow,
                    EndDate = req.RelationshipEndDate ?? DateTime.UtcNow.AddHours(1),
                    Priority = RelationshipPriority.Medium,
                    TenantId = lead.TenantId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = userId,
                    CreationDate = DateTime.UtcNow,
                    CreationAuthorId = userId
                };
                if (sale is not null)
                    relationship.SaleId = sale.Id;
                relationship = await relRepo.AddAsync(relationship);
            }

            if (!string.IsNullOrWhiteSpace(req.Comment))
            {
                var comment = new Comment
                {
                    Text = req.Comment,
                    AuthorId = userId,
                    CreationDate = DateTime.UtcNow
                };
                comment = await commentRepo.AddAsync(comment);
                lead.Comments.Add(comment);
            }

            var oldStatus = lead.Status.ToString();
            lead.Status = LeadStatus.Qualified;
            lead.ContractorId = contractor.Id;
            lead.ConvertDate = DateTime.UtcNow;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = userId;
            if (sale is not null)
                lead.SaleId = sale.Id;
            await leadRepo.UpdateAsync(lead);

            await auditService.LogAsync("Lead", id, "Status", oldStatus, LeadStatus.Qualified.ToString(), userId, req.Comment);

            return Results.Ok(new LeadConversionResult(lead.Id, contractor.Id, sale?.Id, relationship?.Id));
        });

        api.MapGet("/summary", async (AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var query = db.Leads
                .Where(l => l.TenantId == tenantService.TenantId.Value && !l.IsDeleted);

            var summary = new LeadSummary
            {
                TotalNew = await query.CountAsync(l => l.Status == LeadStatus.New),
                TotalInHand = await query.CountAsync(l => l.Status == LeadStatus.InHand),
                TotalQualified = await query.CountAsync(l => l.Status == LeadStatus.Qualified),
                TotalUnqualified = await query.CountAsync(l => l.Status == LeadStatus.Unqualified),
                TotalDuplicate = await query.CountAsync(l => l.Status == LeadStatus.Dublicate),
                TotalConversationNotStart = await query.CountAsync(l => l.Status == LeadStatus.ConversationNotStart),
                TotalLeads = await query.CountAsync()
            };

            return Results.Ok(summary);
        });
    }

    private static long GetUserId(HttpContext http)
    {
        var claim = http.User.FindFirst("userId")?.Value;
        return long.TryParse(claim, out var userId) ? userId : 0;
    }

    private static double CalculateSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0;
        var lowerA = a.ToLowerInvariant();
        var lowerB = b.ToLowerInvariant();
        if (lowerA == lowerB) return 1.0;

        var longer = lowerA.Length > lowerB.Length ? lowerA : lowerB;
        var shorter = lowerA.Length > lowerB.Length ? lowerB : lowerA;

        if (longer.Contains(shorter))
            return (double)shorter.Length / longer.Length;

        var commonChars = longer.Intersect(shorter).Count();
        return (double)commonChars / longer.Length;
    }
}