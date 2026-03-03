using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? TenantId { get; set; }
    public bool Is2faEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public UserStatus Status { get; set; } = UserStatus.EmailNotVerified;
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
}
