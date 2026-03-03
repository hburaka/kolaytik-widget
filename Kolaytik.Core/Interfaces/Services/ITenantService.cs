using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Tenant;

namespace Kolaytik.Core.Interfaces.Services;

public interface ITenantService
{
    Task<PagedResult<TenantResponse>> GetTenantsAsync(PagedRequest request);
    Task<TenantResponse> GetTenantAsync(Guid id);
    Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request, Guid createdBy);
    Task<TenantResponse> UpdateTenantAsync(Guid id, UpdateTenantRequest request);
    Task DeleteTenantAsync(Guid id);
}
