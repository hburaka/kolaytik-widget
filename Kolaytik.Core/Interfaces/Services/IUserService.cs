using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.User;

namespace Kolaytik.Core.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetUsersAsync(PagedRequest request);
    Task<UserResponse> GetUserAsync(Guid id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeleteUserAsync(Guid id);
}
