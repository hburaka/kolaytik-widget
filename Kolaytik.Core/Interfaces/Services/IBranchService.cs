using Kolaytik.Core.DTOs.Branch;
using Kolaytik.Core.DTOs.Common;

namespace Kolaytik.Core.Interfaces.Services;

public interface IBranchService
{
    Task<PagedResult<BranchResponse>> GetBranchesAsync(PagedRequest request);
    Task<BranchResponse> GetBranchAsync(Guid id);
    Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request);
    Task<BranchResponse> UpdateBranchAsync(Guid id, UpdateBranchRequest request);
    Task DeleteBranchAsync(Guid id);
}
