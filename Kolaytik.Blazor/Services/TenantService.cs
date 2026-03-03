using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Tenants;

namespace Kolaytik.Blazor.Services;

public class TenantService : ITenantService
{
    private readonly ApiClient _api;

    public TenantService(ApiClient api) => _api = api;

    public async Task<PagedResult<TenantResponse>?> GetTenantsAsync(PagedRequest request)
    {
        var query = $"page={request.Page}&pageSize={request.PageSize}";
        if (!string.IsNullOrWhiteSpace(request.Search))
            query += $"&search={Uri.EscapeDataString(request.Search)}";
        var result = await _api.GetAsync<PagedResult<TenantResponse>>($"api/tenants?{query}");
        return result?.Data;
    }

    public async Task<TenantResponse?> GetTenantAsync(Guid id)
    {
        var result = await _api.GetAsync<TenantResponse>($"api/tenants/{id}");
        return result?.Data;
    }

    public async Task<TenantResponse?> CreateTenantAsync(CreateTenantRequest request)
    {
        var result = await _api.PostAsync<TenantResponse>("api/tenants", request);
        return result?.Data;
    }

    public async Task<TenantResponse?> UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        var result = await _api.PutAsync<TenantResponse>($"api/tenants/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteTenantAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/tenants/{id}");
        return result?.Success ?? false;
    }
}
