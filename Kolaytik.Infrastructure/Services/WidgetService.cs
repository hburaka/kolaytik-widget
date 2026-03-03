using System.Security.Cryptography;
using System.Text;
using Kolaytik.Core.DTOs.Widget;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Kolaytik.Infrastructure.Services;

public class WidgetService : IWidgetService
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    private const int MaxItems = 100;
    private const int InvalidKeyThreshold = 20;
    private const int IpLockoutMinutes = 15;

    public WidgetService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<WidgetConfigResponse> GetConfigAsync(
        string apiKey, Guid configId, string? origin, string ipAddress)
    {
        var key = await ValidateApiKeyAsync(apiKey, origin, ipAddress);

        var config = await _db.WidgetConfigs
            .Include(w => w.Levels.OrderBy(l => l.OrderIndex))
            .FirstOrDefaultAsync(w => w.Id == configId && w.TenantId == key.TenantId);

        if (config is null)
            throw new KeyNotFoundException("Widget konfigürasyonu bulunamadı.");

        var firstListId = config.Levels.FirstOrDefault()?.ListId;
        if (firstListId.HasValue)
            _ = LogEventAsync(key, firstListId.Value, config.Id, WidgetEventType.Loaded, null, ipAddress);

        return new WidgetConfigResponse
        {
            Id = config.Id,
            Name = config.Name,
            Width = config.Width,
            Levels = config.Levels.Select(l => new WidgetConfigLevelResponse
            {
                OrderIndex = l.OrderIndex,
                ListId = l.ListId,
                ElementType = l.ElementType,
                Label = l.Label,
                Placeholder = l.Placeholder,
                IsRequired = l.IsRequired,
                MaxSelections = l.MaxSelections
            }).ToList()
        };
    }

    public async Task<IList<WidgetItemResponse>> GetItemsAsync(
        string apiKey, Guid listId, Guid? parentItemId, string? origin, string ipAddress)
    {
        var key = await ValidateApiKeyAsync(apiKey, origin, ipAddress);

        var listQuery = _db.Lists.Where(l => l.Id == listId && l.TenantId == key.TenantId);
        if (key.BranchId.HasValue)
            listQuery = listQuery.Where(l => l.BranchId == key.BranchId || l.BranchId == null);

        if (!await listQuery.AnyAsync())
            throw new KeyNotFoundException("Liste bulunamadı.");

        List<ListItem> items;

        if (parentItemId.HasValue)
        {
            items = await _db.ListItemRelations
                .Where(r => r.ParentItemId == parentItemId.Value && r.ChildItem.ListId == listId)
                .OrderBy(r => r.ChildItem.OrderIndex)
                .Take(MaxItems)
                .Select(r => r.ChildItem)
                .ToListAsync();
        }
        else
        {
            items = await _db.ListItems
                .Where(i => i.ListId == listId && i.IsActive)
                .OrderBy(i => i.OrderIndex)
                .Take(MaxItems)
                .ToListAsync();
        }

        var itemIds = items.Select(i => i.Id).ToList();
        var itemsWithChildren = await _db.ListItemRelations
            .Where(r => itemIds.Contains(r.ParentItemId))
            .Select(r => r.ParentItemId)
            .Distinct()
            .ToListAsync();
        var childSet = itemsWithChildren.ToHashSet();

        _ = LogEventAsync(key, listId, null, WidgetEventType.Loaded, null, ipAddress);

        return items.Select(i => new WidgetItemResponse
        {
            Id = i.Id,
            Label = i.Label,
            Value = i.Value,
            Metadata = i.Metadata,
            OrderIndex = i.OrderIndex,
            HasChildren = childSet.Contains(i.Id)
        }).ToList();
    }

    // --- Private helpers ---

    private async Task<ApiKey> ValidateApiKeyAsync(string rawKey, string? origin, string ipAddress)
    {
        CheckIpLockout(ipAddress);

        var keyHash = HashKey(rawKey);
        var apiKey = await _db.ApiKeys
            .Include(k => k.Tenant)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive);

        if (apiKey is null)
        {
            RecordInvalidAttempt(ipAddress);
            throw new UnauthorizedAccessException("Geçersiz veya devre dışı API anahtarı.");
        }

        if (apiKey.Tenant.Status != TenantStatus.Active)
            throw new UnauthorizedAccessException("Tenant aktif değil.");

        if (apiKey.AllowedDomains is { Length: > 0 })
        {
            var requestOrigin = origin ?? string.Empty;
            var allowed = apiKey.AllowedDomains.Any(d =>
                requestOrigin.Contains(d, StringComparison.OrdinalIgnoreCase));
            if (!allowed)
                throw new UnauthorizedAccessException("Bu domain için API anahtarı yetkisi yok.");
        }

        EnforceRateLimit(apiKey);

        _ = UpdateLastUsedAsync(apiKey.Id);

        return apiKey;
    }

    private static string HashKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private void EnforceRateLimit(ApiKey apiKey)
    {
        var now = DateTime.UtcNow;
        var minuteKey = $"widget_rpm:{apiKey.Id}:{now:yyyyMMddHHmm}";
        var dayKey = $"widget_rpd:{apiKey.Id}:{now:yyyyMMdd}";

        var rpmCount = _cache.GetOrCreate(minuteKey, e =>
        {
            e.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(1);
            return 0;
        });

        var rpdCount = _cache.GetOrCreate(dayKey, e =>
        {
            e.AbsoluteExpiration = DateTimeOffset.UtcNow.Date.AddDays(1);
            return 0;
        });

        if (rpmCount >= apiKey.RateLimitPerMinute || rpdCount >= apiKey.RateLimitPerDay)
            throw new InvalidOperationException("Rate limit aşıldı. Lütfen daha sonra tekrar deneyin.");

        _cache.Set(minuteKey, rpmCount + 1, DateTimeOffset.UtcNow.AddMinutes(1));
        _cache.Set(dayKey, rpdCount + 1, DateTimeOffset.UtcNow.Date.AddDays(1));
    }

    private void CheckIpLockout(string ipAddress)
    {
        if (_cache.TryGetValue($"widget_lockout:{ipAddress}", out _))
            throw new UnauthorizedAccessException("Çok fazla geçersiz istek. IP geçici olarak engellendi.");
    }

    private void RecordInvalidAttempt(string ipAddress)
    {
        var attemptsKey = $"widget_invalid:{ipAddress}";
        var lockoutKey = $"widget_lockout:{ipAddress}";

        var attempts = _cache.GetOrCreate(attemptsKey, e =>
        {
            e.SlidingExpiration = TimeSpan.FromMinutes(10);
            return 0;
        });

        attempts++;
        _cache.Set(attemptsKey, attempts, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });

        if (attempts >= InvalidKeyThreshold)
        {
            _cache.Set(lockoutKey, true, TimeSpan.FromMinutes(IpLockoutMinutes));
            _cache.Remove(attemptsKey);
        }
    }

    private async Task UpdateLastUsedAsync(Guid apiKeyId)
    {
        try
        {
            await _db.ApiKeys
                .Where(k => k.Id == apiKeyId)
                .ExecuteUpdateAsync(s => s.SetProperty(k => k.LastUsedAt, DateTime.UtcNow));
        }
        catch { /* fire-and-forget */ }
    }

    private async Task LogEventAsync(ApiKey apiKey, Guid listId, Guid? widgetConfigId,
        WidgetEventType eventType, Guid? selectedItemId, string ipAddress)
    {
        try
        {
            _db.WidgetEvents.Add(new WidgetEvent
            {
                ApiKeyId = apiKey.Id,
                TenantId = apiKey.TenantId,
                WidgetConfigId = widgetConfigId,
                ListId = listId,
                EventType = eventType,
                SelectedItemId = selectedItemId,
                IpAddress = ipAddress
            });
            await _db.SaveChangesAsync();
        }
        catch { /* fire-and-forget */ }
    }
}
