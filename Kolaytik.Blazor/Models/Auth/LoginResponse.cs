namespace Kolaytik.Blazor.Models.Auth;

public class LoginResponse
{
    public bool Requires2fa { get; set; }
    public string? PreAuthToken { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
}
