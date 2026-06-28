using AspireCRM.Web;
using AspireCRM.Web.Components;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOutputCache();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthTokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddTransient<JwtDelegatingHandler>();

var apiBaseUrl = builder.Configuration["ApiServiceUrl"] ?? "https+http://apiservice";

builder.Services.AddHttpClient(string.Empty, client =>
    {
        client.BaseAddress = new(apiBaseUrl);
    });

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new(apiBaseUrl);
    });

builder.Services.AddHttpClient<CrmApiClient>(client =>
    {
        client.BaseAddress = new(apiBaseUrl);
    }).AddHttpMessageHandler<JwtDelegatingHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();