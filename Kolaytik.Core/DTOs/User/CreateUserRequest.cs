using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.User;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? TenantId { get; set; }
    public List<Guid> BranchIds { get; set; } = new();
}
