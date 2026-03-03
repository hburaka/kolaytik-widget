using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.List;

namespace Kolaytik.Core.Interfaces.Services;

public interface IListService
{
    // ── Listeler ──────────────────────────────────────────────────────────
    Task<PagedResult<ListDetailResponse>> GetListsAsync(PagedRequest request);
    Task<ListDetailResponse> GetListAsync(Guid id);
    Task<ListDetailResponse> CreateListAsync(CreateListRequest request);
    Task<ListDetailResponse> UpdateListAsync(Guid id, UpdateListRequest request);
    Task DeleteListAsync(Guid id);

    // ── Liste Elemanları ──────────────────────────────────────────────────
    Task<PagedResult<ListItemResponse>> GetItemsAsync(Guid listId, PagedRequest request);
    Task<ListItemResponse> GetItemAsync(Guid listId, Guid itemId);
    Task<ListItemResponse> CreateItemAsync(Guid listId, CreateListItemRequest request);
    Task<ListItemResponse> UpdateItemAsync(Guid listId, Guid itemId, UpdateListItemRequest request);
    Task DeleteItemAsync(Guid listId, Guid itemId);
    Task ReorderItemsAsync(Guid listId, ReorderItemsRequest request);

    // ── İlişkiler ─────────────────────────────────────────────────────────
    Task<IList<ListItemResponse>> GetChildrenAsync(Guid listId, Guid parentItemId);
    Task SetRelationsAsync(Guid listId, Guid parentItemId, SetRelationsRequest request);
    Task RemoveRelationAsync(Guid listId, Guid parentItemId, Guid childItemId);
}
