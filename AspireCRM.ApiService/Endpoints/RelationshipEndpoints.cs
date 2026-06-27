using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Relationships;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class RelationshipEndpoints
{
    public static void MapRelationshipEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/relationships");

        api.MapGet("/", async (AspireCRMDbContext db, ITenantService tenantService, int page = 1, int pageSize = 20,
            string? type = null, long? leadId = null, long? contractorId = null, long? saleId = null) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var query = db.Relationships
                .Include(r => r.Contractor)
                .Include(r => r.Sale)
                .Include(r => r.Lead)
                .Include(r => r.Contact)
                .Where(r => r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(r => EF.Property<string>(r, "RelationshipType") == type);
            if (leadId.HasValue)
                query = query.Where(r => r.LeadId == leadId);
            if (contractorId.HasValue)
                query = query.Where(r => r.ContractorId == contractorId);
            if (saleId.HasValue)
                query = query.Where(r => r.SaleId == saleId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RelationshipListItem
                {
                    Id = r.Id,
                    Theme = r.Theme,
                    Priority = r.Priority.ToString(),
                    RelationshipType = EF.Property<string>(r, "RelationshipType"),
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    Completed = r.Completed,
                    ContractorName = r.Contractor != null ? r.Contractor.Name : null,
                    SaleName = r.Sale != null ? r.Sale.Name : null,
                    LeadName = r.Lead != null ? r.Lead.Name : null,
                    ContactName = r.Contact != null ? (r.Contact.Firstname + " " + r.Contact.Middlename + " " + r.Contact.Surname) : null
                })
                .ToListAsync();

            return Results.Ok(new { items, totalCount, page, pageSize });
        });

        api.MapGet("/{id:long}", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .Include(r => r.Contractor)
                .Include(r => r.Sale)
                .Include(r => r.Lead)
                .Include(r => r.Contact)
                .Include(r => r.RelationshipUsers)
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            return relationship is null ? Results.NotFound() : Results.Ok(relationship);
        });

        api.MapPost("/", async (CreateRelationshipRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            Relationship relationship = request.Type switch
            {
                "Call" => new RelationshipCall
                {
                    Type = Enum.TryParse<RelationshipCallType>(request.CallType, out var callType) ? callType : RelationshipCallType.Input,
                    UniqueId = request.UniqueId
                },
                "Mail" => new RelationshipMail(),
                "Meeting" => new RelationshipMeeting
                {
                    Place = request.Place,
                    TimeNotSet = request.TimeNotSet
                },
                _ => new Relationship()
            };

            relationship.Theme = request.Theme;
            relationship.Description = request.Description;
            relationship.Priority = Enum.TryParse<RelationshipPriority>(request.Priority, out var priority) ? priority : RelationshipPriority.Medium;
            relationship.StartDate = request.StartDate;
            relationship.EndDate = request.EndDate;
            relationship.ContractorId = request.ContractorId;
            relationship.SaleId = request.SaleId;
            relationship.LeadId = request.LeadId;
            relationship.ContactId = request.ContactId;
            relationship.TenantId = tenantService.TenantId.Value;
            relationship.CreationDate = DateTime.UtcNow;
            relationship.IsPrivate = request.IsPrivate;

            db.Relationships.Add(relationship);
            await db.SaveChangesAsync();

            return Results.Created($"/api/relationships/{relationship.Id}", relationship);
        });

        api.MapPut("/{id:long}", async (long id, UpdateRelationshipRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (relationship is null) return Results.NotFound();

            relationship.Theme = request.Theme;
            relationship.Description = request.Description;
            relationship.Priority = Enum.TryParse<RelationshipPriority>(request.Priority, out var priority) ? priority : RelationshipPriority.Medium;
            relationship.StartDate = request.StartDate;
            relationship.EndDate = request.EndDate;
            relationship.ContractorId = request.ContractorId;
            relationship.SaleId = request.SaleId;
            relationship.LeadId = request.LeadId;
            relationship.ContactId = request.ContactId;
            relationship.ChangeDate = DateTime.UtcNow;
            relationship.IsPrivate = request.IsPrivate;

            if (relationship is RelationshipCall call && request.CallType is not null)
                call.Type = Enum.TryParse<RelationshipCallType>(request.CallType, out var callType) ? callType : call.Type;

            if (relationship is RelationshipMeeting meeting)
            {
                if (request.Place is not null) meeting.Place = request.Place;
                meeting.TimeNotSet = request.TimeNotSet;
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapDelete("/{id:long}", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (relationship is null) return Results.NotFound();

            relationship.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapPost("/{id:long}/complete", async (long id, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (relationship is null) return Results.NotFound();

            relationship.Completed = true;
            relationship.DoneDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // RelationshipUser endpoints
        api.MapGet("/{relationshipId:long}/users", async (long relationshipId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .FirstOrDefaultAsync(r => r.Id == relationshipId && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (relationship is null) return Results.NotFound();

            var users = await db.RelationshipUsers
                .Where(ru => ru.RelationshipId == relationshipId && !ru.IsDeleted)
                .ToListAsync();

            return Results.Ok(users);
        });

        api.MapPost("/{relationshipId:long}/users", async (long relationshipId, AddRelationshipUserRequest request, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationship = await db.Relationships
                .FirstOrDefaultAsync(r => r.Id == relationshipId && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);

            if (relationship is null) return Results.NotFound();

            var existing = await db.RelationshipUsers
                .FirstOrDefaultAsync(ru => ru.RelationshipId == relationshipId && ru.UserId == request.UserId && !ru.IsDeleted);

            if (existing is not null)
                return Results.Conflict("Пользователь уже добавлен как участник");

            var relationshipUser = new RelationshipUser
            {
                RelationshipId = relationshipId,
                UserId = request.UserId,
                Status = request.Status,
                TenantId = tenantService.TenantId.Value
            };

            db.RelationshipUsers.Add(relationshipUser);
            await db.SaveChangesAsync();

            return Results.Created($"/api/relationships/{relationshipId}/users/{relationshipUser.Id}", relationshipUser);
        });

        api.MapDelete("/{relationshipId:long}/users/{userId:long}", async (long relationshipId, long userId, AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var relationshipUser = await db.RelationshipUsers
                .FirstOrDefaultAsync(ru => ru.RelationshipId == relationshipId && ru.UserId == userId && !ru.IsDeleted);

            if (relationshipUser is null) return Results.NotFound();

            relationshipUser.IsDeleted = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public class RelationshipListItem
{
    public long Id { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool? Completed { get; set; }
    public string? ContractorName { get; set; }
    public string? SaleName { get; set; }
    public string? LeadName { get; set; }
    public string? ContactName { get; set; }
}

public class CreateRelationshipRequest
{
    public string Type { get; set; } = "Base";
    public string Theme { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? ContractorId { get; set; }
    public long? SaleId { get; set; }
    public long? LeadId { get; set; }
    public long? ContactId { get; set; }
    public string? CallType { get; set; }
    public string? UniqueId { get; set; }
    public string? Place { get; set; }
    public bool TimeNotSet { get; set; }
    public bool IsPrivate { get; set; }
}

public class UpdateRelationshipRequest
{
    public string Theme { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long? ContractorId { get; set; }
    public long? SaleId { get; set; }
    public long? LeadId { get; set; }
    public long? ContactId { get; set; }
    public string? CallType { get; set; }
    public string? Place { get; set; }
    public bool TimeNotSet { get; set; }
    public bool IsPrivate { get; set; }
}

public class AddRelationshipUserRequest
{
    public long UserId { get; set; }
    public RelationshipUserStatus Status { get; set; }
}