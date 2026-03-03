using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Kolaytik.Blazor.Models.Common;
using Microsoft.AspNetCore.Components;

namespace Kolaytik.Blazor.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _nav;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, ILocalStorageService localStorage, NavigationManager nav)
    {
        _http = http;
        _localStorage = localStorage;
        _nav = nav;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string url)
        => await SendAsync<T>(HttpMethod.Get, url);

    public async Task<ApiResponse<T>?> PostAsync<T>(string url, object? body = null)
        => await SendAsync<T>(HttpMethod.Post, url, body);

    public async Task<ApiResponse<T>?> PutAsync<T>(string url, object? body = null)
        => await SendAsync<T>(HttpMethod.Put, url, body);

    public async Task<ApiResponse?> DeleteAsync(string url)
    {
        var response = await SendRawAsync(HttpMethod.Delete, url);
        if (response is null) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);
    }

    private async Task<ApiResponse<T>?> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        var response = await SendRawAsync(method, url, body);
        if (response is null) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions);
    }

    private async Task<HttpResponseMessage?> SendRawAsync(HttpMethod method, string url, object? body = null)
    {
        var token = await _localStorage.GetItemAsStringAsync("accessToken");
        var request = BuildRequest(method, url, body, token);

        var response = await _http.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Try refresh
            var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
            if (string.IsNullOrEmpty(refreshToken))
            {
                _nav.NavigateTo("/login");
                return null;
            }

            var newToken = await TryRefreshAsync(refreshToken);
            if (newToken is null)
            {
                await _localStorage.RemoveItemAsync("accessToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                _nav.NavigateTo("/login");
                return null;
            }

            // Retry with new token
            request = BuildRequest(method, url, body, newToken);
            response = await _http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _localStorage.RemoveItemAsync("accessToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                _nav.NavigateTo("/login");
                return null;
            }
        }

        return response;
    }

    private static HttpRequestMessage BuildRequest(HttpMethod method, string url, object? body, string? token)
    {
        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        return request;
    }

    private async Task<string?> TryRefreshAsync(string refreshToken)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/refresh", new { refreshToken });
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RefreshResult>(json, JsonOptions);

            if (result?.Data?.AccessToken is null) return null;

            await _localStorage.SetItemAsStringAsync("accessToken", result.Data.AccessToken);
            if (!string.IsNullOrEmpty(result.Data.RefreshToken))
                await _localStorage.SetItemAsStringAsync("refreshToken", result.Data.RefreshToken);

            return result.Data.AccessToken;
        }
        catch
        {
            return null;
        }
    }

    private record TokenData(string AccessToken, string? RefreshToken);
    private record RefreshResult(bool Success, TokenData? Data);
}
