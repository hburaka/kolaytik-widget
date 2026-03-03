using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.User;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponse>>>> GetUsers([FromQuery] PagedRequest request)
    {
        var result = await _userService.GetUsersAsync(request);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUser(Guid id)
    {
        var result = await _userService.GetUserAsync(id);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = result.Id },
            ApiResponse<UserResponse>.Ok(result, "Kullanıcı oluşturuldu."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(ApiResponse<UserResponse>.Ok(result, "Kullanıcı güncellendi."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.Ok("Kullanıcı silindi."));
    }
}
