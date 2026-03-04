using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Sector;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/sectors")]
[Authorize]
public class SectorsController : ControllerBase
{
    private readonly ISectorService _sectorService;

    public SectorsController(ISectorService sectorService)
    {
        _sectorService = sectorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IList<SectorResponse>>>> GetSectors()
    {
        var result = await _sectorService.GetSectorsAsync();
        return Ok(ApiResponse<IList<SectorResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SectorResponse>>> GetSector(Guid id)
    {
        var result = await _sectorService.GetSectorAsync(id);
        return Ok(ApiResponse<SectorResponse>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<SectorResponse>>> CreateSector([FromBody] CreateSectorRequest request)
    {
        var result = await _sectorService.CreateSectorAsync(request);
        return CreatedAtAction(nameof(GetSector), new { id = result.Id },
            ApiResponse<SectorResponse>.Ok(result, "Sektör oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse<SectorResponse>>> UpdateSector(Guid id, [FromBody] UpdateSectorRequest request)
    {
        var result = await _sectorService.UpdateSectorAsync(id, request);
        return Ok(ApiResponse<SectorResponse>.Ok(result, "Sektör güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteSector(Guid id)
    {
        await _sectorService.DeleteSectorAsync(id);
        return Ok(ApiResponse.Ok("Sektör silindi."));
    }
}
