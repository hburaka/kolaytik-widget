using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Kolaytik.Blazor.Auth;
using Kolaytik.Blazor.Models.Auth;
using Kolaytik.Blazor.Models.Common;

namespace Kolaytik.Blazor.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly KolaytikAuthStateProvider _authStateProvider;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthService(HttpClient http, ILocalStorageService localStorage, KolaytikAuthStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, JsonOptions);
        if (result?.Data is null) return null;

        var loginResponse = result.Data;

        if (!loginResponse.Requires2fa && loginResponse.AccessToken is not null)
            await PersistLoginAsync(loginResponse);

        return loginResponse;
    }

    public async Task<LoginResponse?> Verify2faAsync(Verify2faRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/verify-2fa", request);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var err = JsonSerializer.Deserialize<ApiResponse<object>>(json, JsonOptions);
            throw new Exception(err?.Message ?? $"HTTP {(int)response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(json, JsonOptions);
        if (result?.Data?.AccessToken is null) return null;

        await PersistLoginAsync(result.Data);
        return result.Data;
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try { await _http.PostAsJsonAsync("api/auth/logout", new { refreshToken }); } catch { }
        }

        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _authStateProvider.NotifyUserLogoutAsync();
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/change-password");
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        httpRequest.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(httpRequest);
        return response.IsSuccessStatusCode;
    }

    public async Task<TwoFactorSetupResponse?> Setup2faAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/setup-2fa");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<TwoFactorSetupResponse>>(json, JsonOptions);
        return result?.Data;
    }

    public async Task<bool> Confirm2faAsync(string totpCode)
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/confirm-2fa");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new { totpCode });

        var response = await _http.SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    private async Task PersistLoginAsync(LoginResponse loginResponse)
    {
        await _localStorage.SetItemAsStringAsync("accessToken", loginResponse.AccessToken!);
        if (!string.IsNullOrEmpty(loginResponse.RefreshToken))
            await _localStorage.SetItemAsStringAsync("refreshToken", loginResponse.RefreshToken);

        await _authStateProvider.NotifyUserLoginAsync(loginResponse.AccessToken!);
    }
}
