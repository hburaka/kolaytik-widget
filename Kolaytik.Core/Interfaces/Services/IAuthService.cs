using Kolaytik.Core.DTOs.Auth;

namespace Kolaytik.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<LoginResponse> Verify2faAsync(Verify2faRequest request, string ipAddress);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken, Guid userId);
    Task<TwoFactorSetupResponse> Setup2faAsync(Guid userId);
    Task<bool> Confirm2faAsync(Guid userId, string totpCode);
    Task Disable2faAsync(Guid userId, string totpCode);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
