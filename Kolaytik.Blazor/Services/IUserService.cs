using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Users;

namespace Kolaytik.Blazor.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>?> GetUsersAsync(PagedRequest request);
    Task<UserResponse?> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
}
