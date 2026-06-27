using System.Text;
using AspireCRM.ApiService.Endpoints;
using AspireCRM.ApiService.Services;
using AspireCRM.DataLayer;
using AspireCRM.DataLayer.Data;
using AspireCRM.DataLayer.Repositories;
using AspireCRM.Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("aspirecrm") ?? "Data Source=aspirecrm.db";
builder.Services.AddDbContext<AspireCRMDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<CategoryRuleService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FtsIndexingService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole<long>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AspireCRMDbContext>()
.AddDefaultTokenProviders();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"] ?? "AspireCRM",
        ValidAudience = jwtSection["Audience"] ?? "AspireCRM",
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AspireCRMDbContext>();
    db.Database.Migrate();
    await SeedData.InitializeAsync(db);

    try
    {
        var fts = scope.ServiceProvider.GetRequiredService<FtsIndexingService>();
        await fts.RebuildIndexAsync();
    }
    catch
    {
    }
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var tenantClaim = context.User.FindFirst("tenantId")?.Value;
        if (long.TryParse(tenantClaim, out var tenantId))
        {
            var tenantService = context.RequestServices.GetRequiredService<ITenantService>();
            tenantService.SetTenantId(tenantId);
        }
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapAuthEndpoints();
app.MapLookupEndpoints();
app.MapLeadEndpoints();
app.MapContractorEndpoints();
app.MapContactEndpoints();
app.MapSaleEndpoints();
app.MapInpaymentEndpoints();
app.MapProductEndpoints();
app.MapCategoryEndpoints();
app.MapCategoryRuleEndpoints();
app.MapMarketingActivityEndpoints();
app.MapSalesPlanEndpoints();
app.MapSecurityEndpoints();
app.MapRelationshipEndpoints();
app.MapSaleLookupEndpoints();
app.MapAttachmentEndpoints();
app.MapTaskEndpoints();
app.MapCommentEndpoints();
app.MapAuditLogEndpoints();
app.MapSearchEndpoints();

app.MapDefaultEndpoints();

app.Run();