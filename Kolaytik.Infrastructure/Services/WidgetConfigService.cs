using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Widget;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class WidgetConfigService : IWidgetConfigService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public WidgetConfigService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<WidgetConfigManagementResponse>> GetWidgetConfigsAsync(PagedRequest request)
    {
        AssertCanManageWidgetConfigs();

        var query = BuildScope();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .Include(c => c.Tenant)
            .Include(c => c.Levels)
                .ThenInclude(l => l.List)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<WidgetConfigManagementResponse>
        {
            Items = items.Select(ToResponse).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<WidgetConfigManagementResponse> GetWidgetConfigAsync(Guid id)
    {
        AssertCanManageWidgetConfigs();

        var config = await BuildScope()
            .Include(c => c.Tenant)
            .Include(c => c.Levels)
                .ThenInclude(l => l.List)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Widget konfigürasyonu bulunamadı.");

        return ToResponse(config);
    }

    public async Task<WidgetConfigManagementResponse> CreateWidgetConfigAsync(CreateWidgetConfigRequest request)
    {
        AssertCanManageWidgetConfigs();

        var tenantId = ResolveTenantId(request.TenantId);

        var config = new WidgetConfig
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Width = request.Width
        };

        foreach (var levelDto in request.Levels.OrderBy(l => l.OrderIndex))
        {
            config.Levels.Add(new WidgetConfigLevel
            {
                OrderIndex = levelDto.OrderIndex,
                ListId = levelDto.ListId,
                ElementType = ParseElementType(levelDto.ElementType),
                Label = levelDto.Label.Trim(),
                Placeholder = levelDto.Placeholder?.Trim(),
                IsRequired = levelDto.IsRequired,
                MaxSelections = levelDto.MaxSelections
            });
        }

        await _db.WidgetConfigs.AddAsync(config);
        await _db.SaveChangesAsync();

        await _db.Entry(config).Reference(c => c.Tenant).LoadAsync();
        foreach (var level in config.Levels)
            await _db.Entry(level).Reference(l => l.List).LoadAsync();

        return ToResponse(config);
    }

    public async Task<WidgetConfigManagementResponse> UpdateWidgetConfigAsync(Guid id, UpdateWidgetConfigRequest request)
    {
        AssertCanManageWidgetConfigs();

        var config = await BuildScope()
            .Include(c => c.Tenant)
            .Include(c => c.Levels)
                .ThenInclude(l => l.List)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Widget konfigürasyonu bulunamadı.");

        config.Name = request.Name.Trim();
        config.Width = request.Width;
        config.UpdatedAt = DateTime.UtcNow;

        // Mevcut level'ları sil, yenilerini ekle
        _db.WidgetConfigLevels.RemoveRange(config.Levels);
        config.Levels.Clear();

        foreach (var levelDto in request.Levels.OrderBy(l => l.OrderIndex))
        {
            config.Levels.Add(new WidgetConfigLevel
            {
                OrderIndex = levelDto.OrderIndex,
                ListId = levelDto.ListId,
                ElementType = ParseElementType(levelDto.ElementType),
                Label = levelDto.Label.Trim(),
                Placeholder = levelDto.Placeholder?.Trim(),
                IsRequired = levelDto.IsRequired,
                MaxSelections = levelDto.MaxSelections
            });
        }

        await _db.SaveChangesAsync();

        // Level'ların List navigation property'lerini yükle
        foreach (var level in config.Levels)
            await _db.Entry(level).Reference(l => l.List).LoadAsync();

        return ToResponse(config);
    }

    public async Task DeleteWidgetConfigAsync(Guid id)
    {
        AssertCanManageWidgetConfigs();

        var config = await BuildScope()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Widget konfigürasyonu bulunamadı.");

        config.IsDeleted = true;
        config.DeletedAt = DateTime.UtcNow;
        config.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private IQueryable<WidgetConfig> BuildScope()
    {
        var query = _db.WidgetConfigs.AsQueryable();

        if (_currentUser.IsGlobalAdmin)
            return query;

        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

        return query.Where(c => c.TenantId == tenantId);
    }

    private void AssertCanManageWidgetConfigs()
    {
        if (_currentUser.Role is UserRole.BranchManager or UserRole.BranchUser)
            throw new UnauthorizedAccessException("Widget konfigürasyon yönetimi için yetkiniz yok.");
    }

    private Guid ResolveTenantId(Guid? requestedTenantId = null)
    {
        if (_currentUser.IsGlobalAdmin)
            return requestedTenantId ?? throw new ArgumentException("TenantId zorunludur.");

        return _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");
    }

    private static WidgetElementType ParseElementType(string elementType) =>
        elementType switch
        {
            "RadioButton" => WidgetElementType.RadioButton,
            "CheckboxGroup" => WidgetElementType.CheckboxGroup,
            "MultiSelectDropdown" => WidgetElementType.MultiSelectDropdown,
            _ => WidgetElementType.Dropdown
        };

    private static WidgetConfigManagementResponse ToResponse(WidgetConfig c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        TenantName = c.Tenant?.Name ?? string.Empty,
        Name = c.Name,
        Width = c.Width,
        CreatedAt = c.CreatedAt,
        Levels = c.Levels
            .OrderBy(l => l.OrderIndex)
            .Select(l => new WidgetConfigLevelDto
            {
                Id = l.Id,
                OrderIndex = l.OrderIndex,
                ListId = l.ListId,
                ListName = l.List?.Name ?? string.Empty,
                ElementType = l.ElementType.ToString(),
                Label = l.Label,
                Placeholder = l.Placeholder,
                IsRequired = l.IsRequired,
                MaxSelections = l.MaxSelections
            })
            .ToList()
    };
}
