using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspireCRM.DataLayer;
using AspireCRM.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AspireCRM.ApiService.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            UserManager<ApplicationUser> userManager,
            ITenantService tenantService,
            AspireCRMDbContext db) =>
        {
            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == request.TenantCode);
            if (tenant is null)
                return Results.BadRequest("Invalid tenant code");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = tenant.Id
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return Results.BadRequest(result.Errors.Select(e => e.Description));

            return Results.Ok(new { Message = "User registered successfully" });
        })
        .AllowAnonymous();

        group.MapPost("/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !user.IsActive)
                return Results.Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Results.Unauthorized();

            var token = GenerateJwtToken(user, configuration);
            return Results.Ok(new { Token = token, UserId = user.Id, TenantId = user.TenantId });
        })
        .AllowAnonymous();

        group.MapGet("/me", async (ClaimsPrincipal claims, UserManager<ApplicationUser> userManager) =>
        {
            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            return Results.Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.TenantId
            });
        })
        .RequireAuthorization();

        group.MapGet("/users", async (AspireCRMDbContext db, ITenantService tenantService) =>
        {
            if (!tenantService.TenantId.HasValue)
                return Results.Unauthorized();

            var users = await db.Users
                .Where(u => u.TenantId == tenantService.TenantId.Value && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName
                })
                .ToListAsync();

            return Results.Ok(users);
        })
        .RequireAuthorization();

        return group;
    }

    private static string GenerateJwtToken(ApplicationUser user, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
            new Claim(ClaimTypes.Surname, user.LastName ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"] ?? "AspireCRM",
            audience: jwtSection["Audience"] ?? "AspireCRM",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string TenantCode);

public record LoginRequest(string Email, string Password);