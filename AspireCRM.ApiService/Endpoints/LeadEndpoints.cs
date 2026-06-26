using System.Security.Claims;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Leads;

namespace AspireCRM.ApiService.Endpoints;

public record LeadStatusChangeRequest(string? Comment);
public record LeadBatchActivateRequest(long[] Ids);
public record MarkDuplicateRequest(long TargetLeadId, string? Comment);
public record DuplicateCandidate(long Id, string Name, string? Description, LeadStatus Status, double Similarity);

public static class LeadEndpoints
{
    public static void MapLeadEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/leads");

        api.MapGet("/", async (IRepository<Lead> repo, int page = 1, int pageSize = 20, LeadStatus? status = null) =>
        {
            var filter = status.HasValue ? (System.Linq.Expressions.Expression<Func<Lead, bool>>)(l => l.Status == status.Value) : null;
            return Results.Ok(await repo.GetPagedAsync(page, pageSize, filter));
        });

        api.MapGet("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            return lead is null ? Results.NotFound() : Results.Ok(lead);
        });

        api.MapPost("/", async (Lead lead, IRepository<Lead> repo, HttpContext http) =>
        {
            lead.CreationDate = DateTime.UtcNow;
            lead.ChangeDate = DateTime.UtcNow;
            lead.CreationAuthorId = GetUserId(http);
            lead.ChangeAuthorId = GetUserId(http);
            var created = await repo.AddAsync(lead);
            return Results.Created($"/api/leads/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Lead lead, IRepository<Lead> repo, HttpContext http) =>
        {
            if (id != lead.Id) return Results.BadRequest();
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            await repo.DeleteAsync(lead);
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/begin", async (long id, HttpContext http, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            if (lead.Status == LeadStatus.InHand)
                return Results.BadRequest("Лид уже имеет статус \"В работе\"");

            lead.Status = LeadStatus.InHand;
            lead.InHandDate = DateTime.UtcNow;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

            return Results.Ok(lead);
        });

        api.MapPost("/{id:long}/fail", async (long id, LeadStatusChangeRequest req, HttpContext http, IRepository<Lead> repo, IRepository<Comment> commentRepo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();

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

            return Results.Ok(lead);
        });

        api.MapPost("/{id:long}/conversation-not-start", async (long id, HttpContext http, IRepository<Lead> repo) =>
        {
            var lead = await repo.GetByIdAsync(id);
            if (lead is null) return Results.NotFound();
            if (lead.Status == LeadStatus.ConversationNotStart)
                return Results.BadRequest("Лид уже имеет статус \"Разговор не состоялся\"");

            lead.Status = LeadStatus.ConversationNotStart;
            lead.ChangeDate = DateTime.UtcNow;
            lead.ChangeAuthorId = GetUserId(http);
            await repo.UpdateAsync(lead);

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