using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Tenants;

namespace Kolaytik.Blazor.Services;

public interface ITenantService
{
    Task<PagedResult<TenantResponse>?> GetTenantsAsync(PagedRequest request);
    Task<TenantResponse?> GetTenantAsync(Guid id);
    Task<TenantResponse?> CreateTenantAsync(CreateTenantRequest request);
    Task<TenantResponse?> UpdateTenantAsync(Guid id, UpdateTenantRequest request);
    Task<bool> DeleteTenantAsync(Guid id);
}
