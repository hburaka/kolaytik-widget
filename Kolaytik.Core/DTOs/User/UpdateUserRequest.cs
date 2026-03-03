using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.User;

public class UpdateUserRequest
{
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public List<Guid> BranchIds { get; set; } = new();
}
