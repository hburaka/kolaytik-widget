using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Lists;

namespace Kolaytik.Blazor.Services;

public class ListService : IListService
{
    private readonly ApiClient _api;

    public ListService(ApiClient api) => _api = api;

    public async Task<PagedResult<ListResponse>?> GetListsAsync(PagedRequest request)
    {
        var query = BuildQuery(request);
        var result = await _api.GetAsync<PagedResult<ListResponse>>($"api/lists?{query}");
        return result?.Data;
    }

    public async Task<ListResponse?> GetListAsync(Guid id)
    {
        var result = await _api.GetAsync<ListResponse>($"api/lists/{id}");
        return result?.Data;
    }

    public async Task<ListResponse?> CreateListAsync(CreateListRequest request)
    {
        var result = await _api.PostAsync<ListResponse>("api/lists", request);
        return result?.Data;
    }

    public async Task<ListResponse?> UpdateListAsync(Guid id, UpdateListRequest request)
    {
        var result = await _api.PutAsync<ListResponse>($"api/lists/{id}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteListAsync(Guid id)
    {
        var result = await _api.DeleteAsync($"api/lists/{id}");
        return result?.Success ?? false;
    }

    public async Task<PagedResult<ListItemResponse>?> GetItemsAsync(Guid listId, PagedRequest request)
    {
        var query = BuildQuery(request);
        var result = await _api.GetAsync<PagedResult<ListItemResponse>>($"api/lists/{listId}/items?{query}");
        return result?.Data;
    }

    public async Task<ListItemResponse?> CreateItemAsync(Guid listId, CreateListItemRequest request)
    {
        var result = await _api.PostAsync<ListItemResponse>($"api/lists/{listId}/items", request);
        return result?.Data;
    }

    public async Task<ListItemResponse?> UpdateItemAsync(Guid listId, Guid itemId, UpdateListItemRequest request)
    {
        var result = await _api.PutAsync<ListItemResponse>($"api/lists/{listId}/items/{itemId}", request);
        return result?.Data;
    }

    public async Task<bool> DeleteItemAsync(Guid listId, Guid itemId)
    {
        var result = await _api.DeleteAsync($"api/lists/{listId}/items/{itemId}");
        return result?.Success ?? false;
    }

    public async Task<IList<ListItemResponse>?> BulkCreateItemsAsync(Guid listId, BulkCreateItemsRequest request)
    {
        var result = await _api.PostAsync<IList<ListItemResponse>>($"api/lists/{listId}/items/bulk", request);
        return result?.Data;
    }

    public async Task<bool> ReorderItemsAsync(Guid listId, ReorderItemsRequest request)
    {
        var result = await _api.PutAsync<object>($"api/lists/{listId}/items/reorder", request);
        return result?.Success ?? false;
    }

    public async Task<IList<ListItemResponse>?> GetChildrenAsync(Guid listId, Guid parentItemId)
    {
        var result = await _api.GetAsync<IList<ListItemResponse>>($"api/lists/{listId}/items/{parentItemId}/children");
        return result?.Data;
    }

    public async Task<bool> SetRelationsAsync(Guid listId, Guid parentItemId, SetRelationsRequest request)
    {
        var result = await _api.PutAsync<object>($"api/lists/{listId}/items/{parentItemId}/relations", request);
        return result?.Success ?? false;
    }

    private static string BuildQuery(PagedRequest request)
    {
        var parts = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (!string.IsNullOrWhiteSpace(request.Search))
            parts.Add($"search={Uri.EscapeDataString(request.Search)}");
        if (request.BranchId.HasValue)
            parts.Add($"branchId={request.BranchId}");
        return string.Join("&", parts);
    }
}
