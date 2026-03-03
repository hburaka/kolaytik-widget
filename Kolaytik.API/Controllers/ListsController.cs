using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.List;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kolaytik.API.Controllers;

[ApiController]
[Route("api/lists")]
[Authorize]
public class ListsController : ControllerBase
{
    private readonly IListService _listService;

    public ListsController(IListService listService)
    {
        _listService = listService;
    }

    // ── Listeler ──────────────────────────────────────────────────────────

    /// <summary>Erişim kapsamındaki listeleri sayfalı döner. Search ve BranchId ile filtrelenebilir.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ListDetailResponse>>>> GetLists([FromQuery] PagedRequest request)
    {
        var result = await _listService.GetListsAsync(request);
        return Ok(ApiResponse<PagedResult<ListDetailResponse>>.Ok(result));
    }

    /// <summary>Tek listeyi döner.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ListDetailResponse>>> GetList(Guid id)
    {
        var result = await _listService.GetListAsync(id);
        return Ok(ApiResponse<ListDetailResponse>.Ok(result));
    }

    /// <summary>Yeni liste oluşturur. BranchManager sadece kendi şubesi için oluşturabilir.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListDetailResponse>>> CreateList([FromBody] CreateListRequest request)
    {
        var result = await _listService.CreateListAsync(request);
        return CreatedAtAction(nameof(GetList), new { id = result.Id },
            ApiResponse<ListDetailResponse>.Ok(result, "Liste oluşturuldu."));
    }

    /// <summary>Liste adını ve açıklamasını günceller.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ListDetailResponse>>> UpdateList(Guid id, [FromBody] UpdateListRequest request)
    {
        var result = await _listService.UpdateListAsync(id, request);
        return Ok(ApiResponse<ListDetailResponse>.Ok(result, "Liste güncellendi."));
    }

    /// <summary>Listeyi soft-delete ile siler. TenantAdmin+ gerekir.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteList(Guid id)
    {
        await _listService.DeleteListAsync(id);
        return Ok(ApiResponse.Ok("Liste silindi."));
    }

    // ── Liste Elemanları ──────────────────────────────────────────────────

    /// <summary>Listedeki elemanları sayfalı döner.</summary>
    [HttpGet("{listId:guid}/items")]
    public async Task<ActionResult<ApiResponse<PagedResult<ListItemResponse>>>> GetItems(
        Guid listId, [FromQuery] PagedRequest request)
    {
        var result = await _listService.GetItemsAsync(listId, request);
        return Ok(ApiResponse<PagedResult<ListItemResponse>>.Ok(result));
    }

    /// <summary>Tek eleman döner.</summary>
    [HttpGet("{listId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<ListItemResponse>>> GetItem(Guid listId, Guid itemId)
    {
        var result = await _listService.GetItemAsync(listId, itemId);
        return Ok(ApiResponse<ListItemResponse>.Ok(result));
    }

    /// <summary>Yeni eleman ekler.</summary>
    [HttpPost("{listId:guid}/items")]
    public async Task<ActionResult<ApiResponse<ListItemResponse>>> CreateItem(
        Guid listId, [FromBody] CreateListItemRequest request)
    {
        var result = await _listService.CreateItemAsync(listId, request);
        return CreatedAtAction(nameof(GetItem), new { listId, itemId = result.Id },
            ApiResponse<ListItemResponse>.Ok(result, "Eleman eklendi."));
    }

    /// <summary>Eleman bilgilerini günceller.</summary>
    [HttpPut("{listId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse<ListItemResponse>>> UpdateItem(
        Guid listId, Guid itemId, [FromBody] UpdateListItemRequest request)
    {
        var result = await _listService.UpdateItemAsync(listId, itemId, request);
        return Ok(ApiResponse<ListItemResponse>.Ok(result, "Eleman güncellendi."));
    }

    /// <summary>Elemanı soft-delete ile siler. BranchUser hariç tüm roller kullanabilir.</summary>
    [HttpDelete("{listId:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteItem(Guid listId, Guid itemId)
    {
        await _listService.DeleteItemAsync(listId, itemId);
        return Ok(ApiResponse.Ok("Eleman silindi."));
    }

    /// <summary>Elemanların sırasını toplu günceller.</summary>
    [HttpPut("{listId:guid}/items/reorder")]
    public async Task<ActionResult<ApiResponse>> ReorderItems(
        Guid listId, [FromBody] ReorderItemsRequest request)
    {
        await _listService.ReorderItemsAsync(listId, request);
        return Ok(ApiResponse.Ok("Sıralama güncellendi."));
    }

    // ── İlişkiler ─────────────────────────────────────────────────────────

    /// <summary>Bir parent elemanın child'larını döner.</summary>
    [HttpGet("{listId:guid}/items/{parentItemId:guid}/children")]
    public async Task<ActionResult<ApiResponse<IList<ListItemResponse>>>> GetChildren(
        Guid listId, Guid parentItemId)
    {
        var result = await _listService.GetChildrenAsync(listId, parentItemId);
        return Ok(ApiResponse<IList<ListItemResponse>>.Ok(result));
    }

    /// <summary>
    /// Parent-child ilişkilerini toplu olarak ayarlar.
    /// Mevcut tüm ilişkiler silinir, yenileri set edilir.
    /// </summary>
    [HttpPut("{listId:guid}/items/{parentItemId:guid}/relations")]
    public async Task<ActionResult<ApiResponse>> SetRelations(
        Guid listId, Guid parentItemId, [FromBody] SetRelationsRequest request)
    {
        await _listService.SetRelationsAsync(listId, parentItemId, request);
        return Ok(ApiResponse.Ok("İlişkiler güncellendi."));
    }

    /// <summary>Belirli bir parent-child ilişkisini kaldırır.</summary>
    [HttpDelete("{listId:guid}/items/{parentItemId:guid}/relations/{childItemId:guid}")]
    public async Task<ActionResult<ApiResponse>> RemoveRelation(
        Guid listId, Guid parentItemId, Guid childItemId)
    {
        await _listService.RemoveRelationAsync(listId, parentItemId, childItemId);
        return Ok(ApiResponse.Ok("İlişki kaldırıldı."));
    }
}
