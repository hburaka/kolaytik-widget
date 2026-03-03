using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.User;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public List<string> BranchNames { get; set; } = new();
    public UserStatus Status { get; set; }
    public bool Is2faEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
