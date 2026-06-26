using System.Security.Claims;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using AspireCRM.Domain.Leads;

namespace AspireCRM.ApiService.Endpoints;

public record LeadStatusChangeRequest(string? Comment);
public record LeadBatchActivateRequest(long[] Ids);

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

        api.MapPost("/", async (Lead lead, IRepository<Lead> repo) =>
        {
            var created = await repo.AddAsync(lead);
            return Results.Created($"/api/leads/{created.Id}", created);
        });

        api.MapPut("/{id:long}", async (long id, Lead lead, IRepository<Lead> repo) =>
        {
            if (id != lead.Id) return Results.BadRequest();
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
                lead.ChangeDate = DateTime.UtcNow;
                lead.ChangeAuthorId = GetUserId(http);
                await repo.UpdateAsync(lead);
                successCount++;
            }

            return Results.Ok(new { SuccessCount = successCount, Errors = errors });
        });
    }

    private static long GetUserId(HttpContext http)
    {
        var claim = http.User.FindFirst("userId")?.Value;
        return long.TryParse(claim, out var userId) ? userId : 0;
    }
}