using Kolaytik.Blazor.Models.Auth;

namespace Kolaytik.Blazor.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> Verify2faAsync(Verify2faRequest request);
    Task LogoutAsync();
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<TwoFactorSetupResponse?> Setup2faAsync();
    Task<bool> Confirm2faAsync(string totpCode);
}
