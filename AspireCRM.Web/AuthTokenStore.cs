using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AspireCRM.Web;

public class AuthTokenStore(ProtectedLocalStorage localStorage)
{
    private const string TokenKey = "auth_token";

    public async Task<string?> GetTokenAsync()
    {
        var result = await localStorage.GetAsync<string>(TokenKey);
        return result.Success ? result.Value : null;
    }

    public async Task SetTokenAsync(string token)
    {
        await localStorage.SetAsync(TokenKey, token);
    }

    public async Task ClearAsync()
    {
        await localStorage.DeleteAsync(TokenKey);
    }
}

public class JwtAuthStateProvider(AuthTokenStore tokenStore) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void NotifyUserLogout()
    {
        _ = tokenStore.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = Base64UrlDecode(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
        if (keyValuePairs is null) return [];

        return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var length = input.Length;
        var pad = length % 4;
        var padded = pad switch
        {
            2 => input + "==",
            3 => input + "=",
            _ => input
        };
        return Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
    }
}

public class JwtDelegatingHandler(AuthTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}