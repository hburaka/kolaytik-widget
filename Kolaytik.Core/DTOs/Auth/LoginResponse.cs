using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.Auth;

public class LoginResponse
{
    public bool Requires2fa { get; set; }
    /// <summary>Sadece Requires2fa = true olduğunda dolu gelir. Verify-2fa endpoint'ine gönderilir.</summary>
    public string? PreAuthToken { get; set; }
    /// <summary>Admin/SuperAdmin rolü için 2FA kurulmamışsa true. Panel setup sayfasına yönlendirir.</summary>
    public bool MustSetup2fa { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
