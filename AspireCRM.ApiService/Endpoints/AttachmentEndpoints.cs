using AspireCRM.DataLayer;
using AspireCRM.Domain.Attachments;
using Microsoft.EntityFrameworkCore;

namespace AspireCRM.ApiService.Endpoints;

public static class AttachmentEndpoints
{
    public static void MapAttachmentEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/attachments");

        api.MapGet("/by-entity/{entityType}/{entityId:long}", async (
            string entityType, long entityId,
            AspireCRMDbContext db) =>
        {
            var attachments = await db.Attachments
                .Where(a => a.EntityType == entityType && a.EntityId == entityId && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Results.Ok(attachments);
        });

        api.MapGet("/{id:long}/download", async (
            long id,
            AspireCRMDbContext db) =>
        {
            var attachment = await db.Attachments.FindAsync(id);
            if (attachment is null || attachment.IsDeleted)
                return Results.NotFound();

            if (!System.IO.File.Exists(attachment.FilePath))
                return Results.NotFound("Файл не найден на диске");

            var stream = System.IO.File.OpenRead(attachment.FilePath);
            return Results.File(stream, attachment.ContentType, attachment.OriginalName);
        });

        api.MapPost("/upload", async (
            HttpContext http,
            AspireCRMDbContext db,
            ITenantService tenantService) =>
        {
            var form = await http.Request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            var entityType = form["entityType"].FirstOrDefault();
            var entityIdStr = form["entityId"].FirstOrDefault();

            if (file is null || string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityIdStr))
                return Results.BadRequest("Не указан файл, тип сущности или идентификатор");

            if (!long.TryParse(entityIdStr, out var entityId))
                return Results.BadRequest("Некорректный идентификатор сущности");

            var tenantId = tenantService.TenantId ?? 0;
            var userId = GetUserId(http);

            var uploadsDir = System.IO.Path.Combine(app.Environment.ContentRootPath, "App_Data", "Attachments", entityType, entityId.ToString());
            System.IO.Directory.CreateDirectory(uploadsDir);

            var uniqueFileName = $"{Guid.NewGuid():N}{System.IO.Path.GetExtension(file.FileName)}";
            var filePath = System.IO.Path.Combine(uploadsDir, uniqueFileName);

            await using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var attachment = new CrmAttachment
            {
                FileName = uniqueFileName,
                OriginalName = file.FileName,
                FilePath = filePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                EntityType = entityType,
                EntityId = entityId,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            db.Attachments.Add(attachment);
            await db.SaveChangesAsync();

            return Results.Created($"/api/attachments/{attachment.Id}", attachment);
        });

        api.MapDelete("/{id:long}", async (
            long id,
            AspireCRMDbContext db) =>
        {
            var attachment = await db.Attachments.FindAsync(id);
            if (attachment is null || attachment.IsDeleted)
                return Results.NotFound();

            attachment.IsDeleted = true;
            attachment.UpdatedAt = DateTime.UtcNow;

            if (System.IO.File.Exists(attachment.FilePath))
            {
                System.IO.File.Delete(attachment.FilePath);
            }

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
