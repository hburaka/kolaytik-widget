using Kolaytik.Core.DTOs.Auth;
using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// E-posta + şifre ile giriş. 2FA zorunluysa Requires2fa=true ve PreAuthToken döner.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, GetIpAddress());
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    /// <summary>
    /// Pre-auth token + TOTP kodu ile 2FA doğrulaması. Tam token çifti döner.
    /// </summary>
    [HttpPost("verify-2fa")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Verify2fa([FromBody] Verify2faRequest request)
    {
        var result = await _authService.Verify2faAsync(request, GetIpAddress());
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    /// <summary>
    /// Refresh token ile yeni access token alır. Eski refresh token otomatik iptal edilir (rotation).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, GetIpAddress());
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    /// <summary>
    /// Oturumu kapatır; gönderilen refresh token'ı iptal eder.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = GetCurrentUserId();
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken, userId);
        return Ok(ApiResponse.Ok("Oturum kapatıldı."));
    }

    /// <summary>
    /// 2FA kurulumu başlatır. QR kodu URI'sini ve secret'ı döner.
    /// Confirm-2fa ile tamamlanana kadar 2FA aktif olmaz.
    /// </summary>
    [HttpPost("setup-2fa")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TwoFactorSetupResponse>>> Setup2fa()
    {
        var result = await _authService.Setup2faAsync(GetCurrentUserId());
        return Ok(ApiResponse<TwoFactorSetupResponse>.Ok(result));
    }

    /// <summary>
    /// QR kodu tarandıktan sonra TOTP kodu ile 2FA'yı etkinleştirir.
    /// </summary>
    [HttpPost("confirm-2fa")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Confirm2fa([FromBody] ConfirmTwoFactorRequest request)
    {
        var ok = await _authService.Confirm2faAsync(GetCurrentUserId(), request.TotpCode);
        return ok
            ? Ok(ApiResponse.Ok("İki faktörlü doğrulama etkinleştirildi."))
            : BadRequest(ApiResponse.Fail("Geçersiz doğrulama kodu."));
    }

    /// <summary>
    /// 2FA'yı devre dışı bırakır (SuperAdmin/Admin için izin verilmez).
    /// </summary>
    [HttpPost("disable-2fa")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Disable2fa([FromBody] ConfirmTwoFactorRequest request)
    {
        await _authService.Disable2faAsync(GetCurrentUserId(), request.TotpCode);
        return Ok(ApiResponse.Ok("İki faktörlü doğrulama devre dışı bırakıldı."));
    }

    /// <summary>
    /// Oturum açık kullanıcının şifresini değiştirir; tüm refresh token'ları iptal eder.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        await _authService.ChangePasswordAsync(GetCurrentUserId(), request);
        return Ok(ApiResponse.Ok("Şifre başarıyla değiştirildi."));
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException();
    }

    private string GetIpAddress()
    {
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
            return forwarded.ToString().Split(',')[0].Trim();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
