using Kolaytik.Blazor.Models.Branches;
using Kolaytik.Blazor.Models.Common;

namespace Kolaytik.Blazor.Services;

public interface IBranchService
{
    Task<PagedResult<BranchResponse>?> GetBranchesAsync(PagedRequest request);
    Task<BranchResponse?> CreateBranchAsync(CreateBranchRequest request);
    Task<BranchResponse?> UpdateBranchAsync(Guid id, UpdateBranchRequest request);
    Task<bool> DeleteBranchAsync(Guid id);
}
