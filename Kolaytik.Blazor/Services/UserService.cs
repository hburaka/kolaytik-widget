using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Users;

namespace Kolaytik.Blazor.Services;

public class UserService : IUserService
{
    private readonly ApiClient _api;

    public UserService(ApiClient api) => _api = api;

    public async Task<PagedResult<UserResponse>?> GetUsersAsync(PagedRequest request)
    {
        var query = $"page={request.Page}&pageSize={request.PageSize}";
        if (!string.IsNullOrWhiteSpace(request.Search))
            query += $"&search={Uri.EscapeDataString(request.Search)}";
        var result = await _api.GetAsync<PagedResult<UserResponse>>($"api/users?{query}");
        return result?.Data;
    }

    public async Task<UserResponse?> CreateUserAsync(CreateUserRequest request)
    {
        var result = await _api.PostAsync<UserResponse>("api/users", request);
        return result?.Data;
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var result = await _api.PutAsync<UserResponse>($"api/users/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/users/{id}");
        return result?.Success ?? false;
    }
}
