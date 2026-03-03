using Kolaytik.Blazor.Models.Branches;
using Kolaytik.Blazor.Models.Common;

namespace Kolaytik.Blazor.Services;

public class BranchService : IBranchService
{
    private readonly ApiClient _api;

    public BranchService(ApiClient api) => _api = api;

    public async Task<PagedResult<BranchResponse>?> GetBranchesAsync(PagedRequest request)
    {
        var query = $"page={request.Page}&pageSize={request.PageSize}";
        if (!string.IsNullOrWhiteSpace(request.Search))
            query += $"&search={Uri.EscapeDataString(request.Search)}";
        var result = await _api.GetAsync<PagedResult<BranchResponse>>($"api/branches?{query}");
        return result?.Data;
    }

    public async Task<BranchResponse?> CreateBranchAsync(CreateBranchRequest request)
    {
        var result = await _api.PostAsync<BranchResponse>("api/branches", request);
        return result?.Data;
    }

    public async Task<BranchResponse?> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
    {
        var result = await _api.PutAsync<BranchResponse>($"api/branches/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteBranchAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/branches/{id}");
        return result?.Success ?? false;
    }
}
