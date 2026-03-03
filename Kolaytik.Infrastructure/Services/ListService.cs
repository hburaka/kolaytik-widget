using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.List;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Kolaytik.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class ListService : IListService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ListService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    // ── Listeler ──────────────────────────────────────────────────────────

    public async Task<PagedResult<ListDetailResponse>> GetListsAsync(PagedRequest request)
    {
        var query = BuildListScope();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(term)
                                  || (l.Description != null && l.Description.ToLower().Contains(term)));
        }

        if (request.BranchId.HasValue)
            query = query.Where(l => l.BranchId == request.BranchId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new ListDetailResponse
            {
                Id = l.Id,
                Name = l.Name,
                Slug = l.Slug,
                Description = l.Description,
                TenantId = l.TenantId,
                BranchId = l.BranchId,
                BranchName = l.Branch != null ? l.Branch.Name : null,
                ItemCount = l.Items.Count(i => !i.IsDeleted),
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<ListDetailResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ListDetailResponse> GetListAsync(Guid id)
    {
        var list = await BuildListScope()
            .Include(l => l.Branch)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        return ToDetailResponse(list);
    }

    public async Task<ListDetailResponse> CreateListAsync(CreateListRequest request)
    {
        AssertCanManageLists();
        var tenantId = RequireTenantId();

        // BranchManager sadece kendi şubesine liste oluşturabilir
        if (_currentUser.Role == UserRole.BranchManager)
        {
            if (!request.BranchId.HasValue)
                throw new UnauthorizedAccessException("Şube yöneticisi yalnızca şube listesi oluşturabilir.");

            await AssertBranchAccessAsync(request.BranchId.Value);
        }

        if (request.BranchId.HasValue)
            await AssertBranchBelongsToTenantAsync(request.BranchId.Value, tenantId);

        var slug = await GenerateUniqueSlugAsync(request.Name, tenantId);

        var list = new Core.Entities.List
        {
            TenantId = tenantId,
            BranchId = request.BranchId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            CreatedBy = _currentUser.UserId
        };

        await _db.Lists.AddAsync(list);
        await _db.SaveChangesAsync();

        return ToDetailResponse(list);
    }

    public async Task<ListDetailResponse> UpdateListAsync(Guid id, UpdateListRequest request)
    {
        AssertCanManageLists();

        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        // Slug değişmiyorsa sadece adı güncelle
        if (!string.Equals(list.Name, request.Name.Trim(), StringComparison.OrdinalIgnoreCase))
            list.Slug = await GenerateUniqueSlugAsync(request.Name, list.TenantId, excludeListId: id);

        list.Name = request.Name.Trim();
        list.Description = request.Description?.Trim();
        list.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDetailResponse(list);
    }

    public async Task DeleteListAsync(Guid id)
    {
        if (_currentUser.Role == UserRole.BranchManager || _currentUser.Role == UserRole.BranchUser)
            throw new UnauthorizedAccessException("Liste silme yetkiniz yok.");

        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        list.IsDeleted = true;
        list.DeletedAt = DateTime.UtcNow;
        list.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Liste Elemanları ──────────────────────────────────────────────────

    public async Task<PagedResult<ListItemResponse>> GetItemsAsync(Guid listId, PagedRequest request)
    {
        await AssertListReadAccessAsync(listId);

        var query = _db.ListItems.Where(i => i.ListId == listId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(i => i.Label.ToLower().Contains(term)
                                   || i.Value.ToLower().Contains(term));
        }

        var total = await query.CountAsync();

        // HasChildren hesabı için child ID setini çek
        var itemIds = await query.Select(i => i.Id).ToListAsync();
        var parentIds = await _db.ListItemRelations
            .Where(r => itemIds.Contains(r.ParentItemId))
            .Select(r => r.ParentItemId)
            .Distinct()
            .ToListAsync();
        var parentSet = parentIds.ToHashSet();

        var items = await query
            .OrderBy(i => i.OrderIndex).ThenBy(i => i.Label)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<ListItemResponse>
        {
            Items = items.Select(i => ToItemResponse(i, parentSet.Contains(i.Id))),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ListItemResponse> GetItemAsync(Guid listId, Guid itemId)
    {
        await AssertListReadAccessAsync(listId);

        var item = await _db.ListItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ListId == listId)
            ?? throw new KeyNotFoundException("Liste elemanı bulunamadı.");

        var hasChildren = await _db.ListItemRelations.AnyAsync(r => r.ParentItemId == itemId);
        return ToItemResponse(item, hasChildren);
    }

    public async Task<ListItemResponse> CreateItemAsync(Guid listId, CreateListItemRequest request)
    {
        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        var nextOrder = request.OrderIndex
            ?? (await _db.ListItems.Where(i => i.ListId == listId).MaxAsync(i => (int?)i.OrderIndex) ?? 0) + 1;

        var item = new ListItem
        {
            ListId = listId,
            TenantId = list.TenantId,
            Label = request.Label.Trim(),
            Value = request.Value.Trim(),
            Metadata = request.Metadata,
            OrderIndex = nextOrder,
            IsActive = request.IsActive,
            CreatedBy = _currentUser.UserId
        };

        await _db.ListItems.AddAsync(item);
        await _db.SaveChangesAsync();
        return ToItemResponse(item, false);
    }

    public async Task<ListItemResponse> UpdateItemAsync(Guid listId, Guid itemId, UpdateListItemRequest request)
    {
        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        var item = await _db.ListItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ListId == listId)
            ?? throw new KeyNotFoundException("Liste elemanı bulunamadı.");

        item.Label = request.Label.Trim();
        item.Value = request.Value.Trim();
        item.Metadata = request.Metadata;
        item.OrderIndex = request.OrderIndex;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var hasChildren = await _db.ListItemRelations.AnyAsync(r => r.ParentItemId == itemId);
        return ToItemResponse(item, hasChildren);
    }

    public async Task DeleteItemAsync(Guid listId, Guid itemId)
    {
        if (!_currentUser.CanDeleteItems)
            throw new UnauthorizedAccessException("Liste elemanı silme yetkiniz yok.");

        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        var item = await _db.ListItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ListId == listId)
            ?? throw new KeyNotFoundException("Liste elemanı bulunamadı.");

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ReorderItemsAsync(Guid listId, ReorderItemsRequest request)
    {
        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        var items = await _db.ListItems
            .Where(i => i.ListId == listId && request.ItemIds.Contains(i.Id))
            .ToListAsync();

        for (int i = 0; i < request.ItemIds.Count; i++)
        {
            var item = items.FirstOrDefault(x => x.Id == request.ItemIds[i]);
            if (item is not null)
            {
                item.OrderIndex = i + 1;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    // ── İlişkiler ─────────────────────────────────────────────────────────

    public async Task<IList<ListItemResponse>> GetChildrenAsync(Guid listId, Guid parentItemId)
    {
        await AssertListReadAccessAsync(listId);

        var children = await _db.ListItemRelations
            .Where(r => r.ParentItemId == parentItemId)
            .Include(r => r.ChildItem)
            .Select(r => r.ChildItem)
            .Where(i => !i.IsDeleted && i.IsActive)
            .OrderBy(i => i.OrderIndex)
            .ToListAsync();

        // Torunları olan child'ları bul
        var childIds = children.Select(c => c.Id).ToList();
        var hasChildrenSet = (await _db.ListItemRelations
            .Where(r => childIds.Contains(r.ParentItemId))
            .Select(r => r.ParentItemId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        return children.Select(i => ToItemResponse(i, hasChildrenSet.Contains(i.Id))).ToList();
    }

    public async Task SetRelationsAsync(Guid listId, Guid parentItemId, SetRelationsRequest request)
    {
        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        // Parent item bu listeye ait mi?
        var parentExists = await _db.ListItems
            .AnyAsync(i => i.Id == parentItemId && i.ListId == listId);
        if (!parentExists)
            throw new KeyNotFoundException("Parent eleman bulunamadı.");

        // Child item'lar aynı tenant'a ait mi?
        if (request.ChildItemIds.Count > 0)
        {
            var validCount = await _db.ListItems
                .CountAsync(i => request.ChildItemIds.Contains(i.Id) && i.TenantId == list.TenantId);
            if (validCount != request.ChildItemIds.Count)
                throw new ArgumentException("Bazı child elemanlar geçersiz veya erişilemez.");
        }

        // Mevcut ilişkileri sil
        var existing = await _db.ListItemRelations
            .Where(r => r.ParentItemId == parentItemId)
            .ToListAsync();
        _db.ListItemRelations.RemoveRange(existing);

        // Yeni ilişkileri ekle
        var newRelations = request.ChildItemIds.Select(childId => new ListItemRelation
        {
            ParentItemId = parentItemId,
            ChildItemId = childId
        });
        await _db.ListItemRelations.AddRangeAsync(newRelations);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveRelationAsync(Guid listId, Guid parentItemId, Guid childItemId)
    {
        var list = await BuildListScope()
            .FirstOrDefaultAsync(l => l.Id == listId)
            ?? throw new KeyNotFoundException("Liste bulunamadı.");

        AssertListWriteAccess(list);

        var relation = await _db.ListItemRelations
            .FirstOrDefaultAsync(r => r.ParentItemId == parentItemId && r.ChildItemId == childItemId)
            ?? throw new KeyNotFoundException("İlişki bulunamadı.");

        _db.ListItemRelations.Remove(relation);
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı metodlar ─────────────────────────────────────────────────

    /// <summary>Kullanıcının rolüne göre erişebileceği listeleri filtreler.</summary>
    private IQueryable<Core.Entities.List> BuildListScope()
    {
        var query = _db.Lists.AsQueryable();

        if (_currentUser.IsGlobalAdmin)
            return query; // SuperAdmin/Admin tüm tenant'ları görür

        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

        query = query.Where(l => l.TenantId == tenantId);

        // BranchManager ve BranchUser sadece kendi şubelerini + tenant geneli listeleri görür
        if (_currentUser.Role is UserRole.BranchManager or UserRole.BranchUser)
        {
            var userBranchIds = _db.UserBranches
                .Where(ub => ub.UserId == _currentUser.UserId)
                .Select(ub => ub.BranchId);

            query = query.Where(l => l.BranchId == null || userBranchIds.Contains(l.BranchId.Value));
        }

        return query;
    }

    private void AssertCanManageLists()
    {
        if (!_currentUser.CanManageLists)
            throw new UnauthorizedAccessException("Liste yönetimi için yetkiniz yok.");
    }

    private void AssertListWriteAccess(Core.Entities.List list)
    {
        if (_currentUser.IsGlobalAdmin) return;

        if (list.TenantId != _currentUser.TenantId)
            throw new UnauthorizedAccessException("Bu listeye erişim yetkiniz yok.");

        // BranchManager sadece kendi şube listesine yazabilir, tenant geneline yazamaz
        if (_currentUser.Role == UserRole.BranchManager && list.BranchId == null)
            throw new UnauthorizedAccessException("Tenant geneli listeyi düzenleme yetkiniz yok.");
    }

    private async Task AssertListReadAccessAsync(Guid listId)
    {
        var exists = await BuildListScope().AnyAsync(l => l.Id == listId);
        if (!exists)
            throw new KeyNotFoundException("Liste bulunamadı.");
    }

    private async Task AssertBranchAccessAsync(Guid branchId)
    {
        var hasAccess = await _db.UserBranches
            .AnyAsync(ub => ub.UserId == _currentUser.UserId && ub.BranchId == branchId);
        if (!hasAccess)
            throw new UnauthorizedAccessException("Bu şubeye erişim yetkiniz yok.");
    }

    private async Task AssertBranchBelongsToTenantAsync(Guid branchId, Guid tenantId)
    {
        var belongs = await _db.Branches
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId);
        if (!belongs)
            throw new ArgumentException("Şube bu firmaya ait değil.");
    }

    private Guid RequireTenantId() =>
        _currentUser.TenantId ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid tenantId, Guid? excludeListId = null)
    {
        var baseSlug = SlugHelper.ToSlug(name);
        var slug = baseSlug;
        var attempt = 0;

        while (true)
        {
            var q = _db.Lists.Where(l => l.TenantId == tenantId && l.Slug == slug);
            if (excludeListId.HasValue)
                q = q.Where(l => l.Id != excludeListId.Value);

            if (!await q.AnyAsync())
                return slug;

            attempt++;
            slug = $"{baseSlug}-{attempt}";
        }
    }

    private static ListDetailResponse ToDetailResponse(Core.Entities.List l) => new()
    {
        Id = l.Id,
        Name = l.Name,
        Slug = l.Slug,
        Description = l.Description,
        TenantId = l.TenantId,
        BranchId = l.BranchId,
        BranchName = l.Branch?.Name,
        ItemCount = l.Items.Count(i => !i.IsDeleted),
        CreatedAt = l.CreatedAt,
        UpdatedAt = l.UpdatedAt
    };

    private static ListItemResponse ToItemResponse(ListItem i, bool hasChildren) => new()
    {
        Id = i.Id,
        ListId = i.ListId,
        Label = i.Label,
        Value = i.Value,
        Metadata = i.Metadata,
        OrderIndex = i.OrderIndex,
        IsActive = i.IsActive,
        HasChildren = hasChildren,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt
    };

}
