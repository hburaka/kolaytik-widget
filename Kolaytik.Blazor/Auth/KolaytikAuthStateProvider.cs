using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Kolaytik.Blazor.Auth;

public class KolaytikAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public KolaytikAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        if (string.IsNullOrWhiteSpace(token))
            return AnonymousState;

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(user);
    }

    public async Task NotifyUserLoginAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync("accessToken", token);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    public async Task<string?> GetEmailAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        if (string.IsNullOrEmpty(token)) return null;
        var claims = ParseClaimsFromJwt(token);
        return claims.FirstOrDefault(c => c.Type == "email")?.Value;
    }

    public async Task<string?> GetRoleAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        if (string.IsNullOrEmpty(token)) return null;
        var claims = ParseClaimsFromJwt(token);
        return claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
    }

    public async Task<Guid?> GetTenantIdAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        if (string.IsNullOrEmpty(token)) return null;
        var claims = ParseClaimsFromJwt(token);
        var tenantIdStr = claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
        return Guid.TryParse(tenantIdStr, out var id) ? id : null;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)
                            ?? new Dictionary<string, JsonElement>();

        return keyValuePairs.Select(kvp =>
            new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty));
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
