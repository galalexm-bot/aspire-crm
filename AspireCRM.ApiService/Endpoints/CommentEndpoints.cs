using AspireCRM.DataLayer;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Contractors;
using AspireCRM.Domain.Leads;
using AspireCRM.Domain.Payments;
using AspireCRM.Domain.Relationships;
using AspireCRM.Domain.Sales;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/comments");

        api.MapGet("/{entityType}/{entityId:long}", async (string entityType, long entityId,
            AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            IQueryable<Comment> query = entityType.ToLower() switch
            {
                "lead" => db.Comments.Where(c => c.Leads.Any(l => l.Id == entityId)),
                "sale" => db.Comments.Where(c => c.Sales.Any(s => s.Id == entityId)),
                "contractor" => db.Comments.Where(c => c.Contractors.Any(co => co.Id == entityId)),
                "inpayment" => db.Comments.Where(c => c.Inpayments.Any(i => i.Id == entityId)),
                "relationship" => db.Comments.Where(c => c.Relationships.Any(r => r.Id == entityId)),
                _ => Enumerable.Empty<Comment>().AsQueryable()
            };

            if (!query.Any())
                return Results.Ok(new List<CommentDto>());

            var comments = await query
                .OrderByDescending(c => c.CreationDate)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    AuthorId = c.AuthorId,
                    CreationDate = c.CreationDate,
                    AuthorName = ""
                })
                .ToListAsync();

            var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
            var users = await db.Users
                .Where(u => authorIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
                .ToListAsync();

            var userNames = users.ToDictionary(u => u.Id, u => u.Name.Trim());

            foreach (var c in comments)
                c.AuthorName = userNames.GetValueOrDefault(c.AuthorId, $"#{c.AuthorId}");

            return Results.Ok(comments);
        });

        api.MapPost("/{entityType}/{entityId:long}", async (string entityType, long entityId,
            CreateCommentRequest request, AspireCRMDbContext db,
            ITenantService tenantService, HttpContext http) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Text))
                return Results.BadRequest("Текст комментария не может быть пустым");

            var userId = GetUserId(http);

            var comment = new Comment
            {
                Text = request.Text.Trim(),
                AuthorId = userId,
                CreationDate = DateTime.UtcNow,
                TenantId = tenantService.TenantId.Value
            };

            switch (entityType.ToLower())
            {
                case "lead":
                    var lead = await db.Leads.FirstOrDefaultAsync(l => l.Id == entityId && l.TenantId == tenantService.TenantId.Value && !l.IsDeleted);
                    if (lead is null) return Results.NotFound();
                    comment.Leads.Add(lead);
                    break;
                case "sale":
                    var sale = await db.Sales.FirstOrDefaultAsync(s => s.Id == entityId && s.TenantId == tenantService.TenantId.Value && !s.IsDeleted);
                    if (sale is null) return Results.NotFound();
                    comment.Sales.Add(sale);
                    break;
                case "contractor":
                    var contractor = await db.Contractors.FirstOrDefaultAsync(c => c.Id == entityId && c.TenantId == tenantService.TenantId.Value && !c.IsDeleted);
                    if (contractor is null) return Results.NotFound();
                    comment.Contractors.Add(contractor);
                    break;
                case "inpayment":
                    var inpayment = await db.Inpayments.FirstOrDefaultAsync(i => i.Id == entityId && i.TenantId == tenantService.TenantId.Value && !i.IsDeleted);
                    if (inpayment is null) return Results.NotFound();
                    comment.Inpayments.Add(inpayment);
                    break;
                case "relationship":
                    var relationship = await db.Relationships.FirstOrDefaultAsync(r => r.Id == entityId && r.TenantId == tenantService.TenantId.Value && !r.IsDeleted);
                    if (relationship is null) return Results.NotFound();
                    comment.Relationships.Add(relationship);
                    break;
                default:
                    return Results.BadRequest($"Неизвестный тип сущности: {entityType}");
            }

            db.Comments.Add(comment);
            await db.SaveChangesAsync();

            var author = await db.Users.Where(u => u.Id == userId).Select(u => u.FirstName + " " + u.LastName).FirstOrDefaultAsync();

            return Results.Created($"/api/comments/{comment.Id}", new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                AuthorId = comment.AuthorId,
                AuthorName = (author ?? "").Trim(),
                CreationDate = comment.CreationDate
            });
        });

        api.MapDelete("/{id:long}", async (long id, AspireCRMDbContext db,
            ITenantService tenantService, HttpContext http) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var comment = await db.Comments
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantService.TenantId.Value);

            if (comment is null) return Results.NotFound();

            var userId = GetUserId(http);
            if (comment.AuthorId != userId)
                return Results.Forbid();

            db.Comments.Remove(comment);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    private static long GetUserId(HttpContext http)
    {
        var claim = http.User.FindFirst("userId")?.Value;
        return long.TryParse(claim, out var userId) ? userId : 0;
    }
}

public class CommentDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}