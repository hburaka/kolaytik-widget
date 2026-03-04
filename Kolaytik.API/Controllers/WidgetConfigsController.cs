using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Widget;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/widget-configs")]
[Authorize(Roles = "SuperAdmin,Admin,TenantAdmin")]
public class WidgetConfigsController : ControllerBase
{
    private readonly IWidgetConfigService _widgetConfigService;

    public WidgetConfigsController(IWidgetConfigService widgetConfigService)
    {
        _widgetConfigService = widgetConfigService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<WidgetConfigManagementResponse>>>> GetWidgetConfigs([FromQuery] PagedRequest request)
    {
        var result = await _widgetConfigService.GetWidgetConfigsAsync(request);
        return Ok(ApiResponse<PagedResult<WidgetConfigManagementResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WidgetConfigManagementResponse>>> GetWidgetConfig(Guid id)
    {
        var result = await _widgetConfigService.GetWidgetConfigAsync(id);
        return Ok(ApiResponse<WidgetConfigManagementResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<WidgetConfigManagementResponse>>> CreateWidgetConfig([FromBody] CreateWidgetConfigRequest request)
    {
        var result = await _widgetConfigService.CreateWidgetConfigAsync(request);
        return CreatedAtAction(nameof(GetWidgetConfig), new { id = result.Id },
            ApiResponse<WidgetConfigManagementResponse>.Ok(result, "Widget konfigürasyonu oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WidgetConfigManagementResponse>>> UpdateWidgetConfig(Guid id, [FromBody] UpdateWidgetConfigRequest request)
    {
        var result = await _widgetConfigService.UpdateWidgetConfigAsync(id, request);
        return Ok(ApiResponse<WidgetConfigManagementResponse>.Ok(result, "Widget konfigürasyonu güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteWidgetConfig(Guid id)
    {
        await _widgetConfigService.DeleteWidgetConfigAsync(id);
        return Ok(ApiResponse.Ok("Widget konfigürasyonu silindi."));
    }
}
