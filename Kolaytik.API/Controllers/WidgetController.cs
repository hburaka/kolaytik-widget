using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/widget")]
[AllowAnonymous]
public class WidgetController : ControllerBase
{
    private readonly IWidgetService _widgetService;

    public WidgetController(IWidgetService widgetService)
    {
        _widgetService = widgetService;
    }

    /// <summary>Widget konfigürasyonunu döner.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig([FromQuery] string api_key, [FromQuery] Guid config_id)
    {
        if (string.IsNullOrWhiteSpace(api_key))
            return BadRequest(new { success = false, message = "api_key zorunludur." });

        var origin = Request.Headers.Origin.FirstOrDefault();
        var ip = GetClientIp();

        var result = await _widgetService.GetConfigAsync(api_key, config_id, origin, ip);
        return Ok(new { success = true, data = result });
    }

    /// <summary>Liste elemanlarını döner. parent_item_id verilirse child elemanları döner.</summary>
    [HttpGet("items")]
    public async Task<IActionResult> GetItems(
        [FromQuery] string api_key,
        [FromQuery] Guid list_id,
        [FromQuery] Guid? parent_item_id = null)
    {
        if (string.IsNullOrWhiteSpace(api_key))
            return BadRequest(new { success = false, message = "api_key zorunludur." });

        var origin = Request.Headers.Origin.FirstOrDefault();
        var ip = GetClientIp();

        var result = await _widgetService.GetItemsAsync(api_key, list_id, parent_item_id, origin, ip);
        return Ok(new { success = true, data = result });
    }

    private string GetClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
            return forwarded.Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
