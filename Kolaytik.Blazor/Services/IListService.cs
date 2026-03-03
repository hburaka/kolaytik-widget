using Kolaytik.Blazor.Models.Common;
using Kolaytik.Blazor.Models.Lists;

namespace Kolaytik.Blazor.Services;

public interface IListService
{
    Task<PagedResult<ListResponse>?> GetListsAsync(PagedRequest request);
    Task<ListResponse?> GetListAsync(Guid id);
    Task<ListResponse?> CreateListAsync(CreateListRequest request);
    Task<ListResponse?> UpdateListAsync(Guid id, UpdateListRequest request);
    Task<bool> DeleteListAsync(Guid id);

    Task<PagedResult<ListItemResponse>?> GetItemsAsync(Guid listId, PagedRequest request);
    Task<ListItemResponse?> CreateItemAsync(Guid listId, CreateListItemRequest request);
    Task<ListItemResponse?> UpdateItemAsync(Guid listId, Guid itemId, UpdateListItemRequest request);
    Task<bool> DeleteItemAsync(Guid listId, Guid itemId);
    Task<bool> ReorderItemsAsync(Guid listId, ReorderItemsRequest request);
}
