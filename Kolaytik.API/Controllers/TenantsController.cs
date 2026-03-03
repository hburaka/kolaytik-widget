using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Tenant;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUser;

    public TenantsController(ITenantService tenantService, ICurrentUserService currentUser)
    {
        _tenantService = tenantService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TenantResponse>>>> GetTenants([FromQuery] PagedRequest request)
    {
        var result = await _tenantService.GetTenantsAsync(request);
        return Ok(ApiResponse<PagedResult<TenantResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> GetTenant(Guid id)
    {
        var result = await _tenantService.GetTenantAsync(id);
        return Ok(ApiResponse<TenantResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        var result = await _tenantService.CreateTenantAsync(request, _currentUser.UserId);
        return CreatedAtAction(nameof(GetTenant), new { id = result.Id },
            ApiResponse<TenantResponse>.Ok(result, "Firma oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        var result = await _tenantService.UpdateTenantAsync(id, request);
        return Ok(ApiResponse<TenantResponse>.Ok(result, "Firma güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteTenant(Guid id)
    {
        await _tenantService.DeleteTenantAsync(id);
        return Ok(ApiResponse.Ok("Firma silindi."));
    }
}
