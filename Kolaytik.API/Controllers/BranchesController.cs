using Kolaytik.Core.DTOs.Branch;
using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BranchResponse>>>> GetBranches([FromQuery] PagedRequest request)
    {
        var result = await _branchService.GetBranchesAsync(request);
        return Ok(ApiResponse<PagedResult<BranchResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> GetBranch(Guid id)
    {
        var result = await _branchService.GetBranchAsync(id);
        return Ok(ApiResponse<BranchResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> CreateBranch([FromBody] CreateBranchRequest request)
    {
        var result = await _branchService.CreateBranchAsync(request);
        return CreatedAtAction(nameof(GetBranch), new { id = result.Id },
            ApiResponse<BranchResponse>.Ok(result, "Şube oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BranchResponse>>> UpdateBranch(Guid id, [FromBody] UpdateBranchRequest request)
    {
        var result = await _branchService.UpdateBranchAsync(id, request);
        return Ok(ApiResponse<BranchResponse>.Ok(result, "Şube güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteBranch(Guid id)
    {
        await _branchService.DeleteBranchAsync(id);
        return Ok(ApiResponse.Ok("Şube silindi."));
    }
}
