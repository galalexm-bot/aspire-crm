using AspireCRM.ApiService.Services;
using AspireCRM.Domain.Security;

namespace AspireCRM.ApiService.Endpoints;

public static class PermissionEndpointFilter
{
    public static RouteHandlerBuilder RequireCategoryPermission(this RouteHandlerBuilder builder, CategoryPermissionLevel minimumLevel)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var userId = permissionService.GetCurrentUserId();

            if (userId == 0)
                return TypedResults.Unauthorized();

            if (minimumLevel == CategoryPermissionLevel.None)
                return await next(context);

            var hasAny = await permissionService.HasAnyCategoryPermissionAsync(userId, minimumLevel);
            if (!hasAny)
                return Results.Json(new { error = "Недостаточно прав доступа" }, statusCode: 403);

            return await next(context);
        });
    }
}