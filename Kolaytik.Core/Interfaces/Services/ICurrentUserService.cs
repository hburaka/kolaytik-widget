using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Interfaces.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid? TenantId { get; }
    UserRole Role { get; }

    bool IsGlobalAdmin => Role is UserRole.SuperAdmin or UserRole.Admin;
    bool CanDeleteItems => Role is not UserRole.BranchUser;
    bool CanManageLists => Role is not UserRole.BranchUser;
}
