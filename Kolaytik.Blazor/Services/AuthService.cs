using System.Net.Http.Json;
using Kolaytik.Blazor.Auth;

namespace Kolaytik.Blazor.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly KolaytikAuthStateProvider _authStateProvider;

    public AuthService(HttpClient http, KolaytikAuthStateProvider authStateProvider)
    {
        _http = http;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(string email, string password, string? totpCode = null)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new
        {
            email,
            password,
            totpCode
        });

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result?.AccessToken is null)
            return false;

        await _authStateProvider.NotifyUserLoginAsync(result.AccessToken);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.NotifyUserLogoutAsync();
    }

    private record LoginResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);
}
