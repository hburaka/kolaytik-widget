using Kolaytik.Core.DTOs.ApiKey;
using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/api-keys")]
[Authorize(Roles = "SuperAdmin,Admin,TenantAdmin")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeysController(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IList<ApiKeyResponse>>>> GetApiKeys()
    {
        var result = await _apiKeyService.GetApiKeysAsync();
        return Ok(ApiResponse<IList<ApiKeyResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateApiKeyResponse>>> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        var result = await _apiKeyService.CreateApiKeyAsync(request);
        return Ok(ApiResponse<CreateApiKeyResponse>.Ok(result, "API anahtarı oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> UpdateApiKey(Guid id, [FromBody] UpdateApiKeyRequest request)
    {
        var result = await _apiKeyService.UpdateApiKeyAsync(id, request);
        return Ok(ApiResponse<ApiKeyResponse>.Ok(result, "API anahtarı güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteApiKey(Guid id)
    {
        await _apiKeyService.DeleteApiKeyAsync(id);
        return Ok(ApiResponse.Ok("API anahtarı silindi."));
    }
}
