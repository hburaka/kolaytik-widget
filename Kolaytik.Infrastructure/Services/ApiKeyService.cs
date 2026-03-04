using System.Security.Cryptography;
using System.Text;
using Kolaytik.Core.DTOs.ApiKey;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ApiKeyService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IList<ApiKeyResponse>> GetApiKeysAsync()
    {
        var query = BuildApiKeyScope();

        return await query
            .Include(k => k.Tenant)
            .Include(k => k.Branch)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyResponse
            {
                Id = k.Id,
                TenantId = k.TenantId,
                TenantName = k.Tenant.Name,
                BranchId = k.BranchId,
                BranchName = k.Branch != null ? k.Branch.Name : null,
                Name = k.Name,
                KeyPrefix = k.KeyPrefix,
                IsActive = k.IsActive,
                RateLimitPerMinute = k.RateLimitPerMinute,
                RateLimitPerDay = k.RateLimitPerDay,
                AllowedDomains = k.AllowedDomains != null ? k.AllowedDomains.ToList() : new List<string>(),
                CreatedAt = k.CreatedAt,
                LastUsedAt = k.LastUsedAt
            })
            .ToListAsync();
    }

    public async Task<ApiKeyResponse> GetApiKeyAsync(Guid id)
    {
        var apiKey = await BuildApiKeyScope()
            .Include(k => k.Tenant)
            .Include(k => k.Branch)
            .FirstOrDefaultAsync(k => k.Id == id)
            ?? throw new KeyNotFoundException("API anahtarı bulunamadı.");

        return ToResponse(apiKey);
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        AssertCanManageApiKeys();

        var tenantId = ResolveTenantId();

        // Generate 32-byte random key
        var plainKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var keyPrefix = plainKey[..8];
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plainKey))).ToLowerInvariant();

        var apiKey = new ApiKey
        {
            TenantId = tenantId,
            BranchId = request.BranchId,
            Name = request.Name.Trim(),
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            IsActive = true,
            RateLimitPerMinute = request.RateLimitPerMinute,
            RateLimitPerDay = request.RateLimitPerDay,
            AllowedDomains = request.AllowedDomains.Count > 0 ? request.AllowedDomains.ToArray() : null
        };

        await _db.ApiKeys.AddAsync(apiKey);
        await _db.SaveChangesAsync();

        await _db.Entry(apiKey).Reference(k => k.Tenant).LoadAsync();
        if (apiKey.BranchId.HasValue)
            await _db.Entry(apiKey).Reference(k => k.Branch).LoadAsync();

        return new CreateApiKeyResponse
        {
            ApiKey = ToResponse(apiKey),
            PlainKey = plainKey
        };
    }

    public async Task<ApiKeyResponse> UpdateApiKeyAsync(Guid id, UpdateApiKeyRequest request)
    {
        AssertCanManageApiKeys();

        var apiKey = await BuildApiKeyScope()
            .Include(k => k.Tenant)
            .Include(k => k.Branch)
            .FirstOrDefaultAsync(k => k.Id == id)
            ?? throw new KeyNotFoundException("API anahtarı bulunamadı.");

        apiKey.Name = request.Name.Trim();
        apiKey.IsActive = request.IsActive;
        apiKey.RateLimitPerMinute = request.RateLimitPerMinute;
        apiKey.RateLimitPerDay = request.RateLimitPerDay;
        apiKey.AllowedDomains = request.AllowedDomains.Count > 0 ? request.AllowedDomains.ToArray() : null;
        apiKey.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToResponse(apiKey);
    }

    public async Task DeleteApiKeyAsync(Guid id)
    {
        AssertCanManageApiKeys();

        var apiKey = await BuildApiKeyScope()
            .FirstOrDefaultAsync(k => k.Id == id)
            ?? throw new KeyNotFoundException("API anahtarı bulunamadı.");

        apiKey.IsDeleted = true;
        apiKey.DeletedAt = DateTime.UtcNow;
        apiKey.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private IQueryable<ApiKey> BuildApiKeyScope()
    {
        var query = _db.ApiKeys.AsQueryable();

        if (_currentUser.IsGlobalAdmin)
            return query;

        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

        return query.Where(k => k.TenantId == tenantId);
    }

    private void AssertCanManageApiKeys()
    {
        if (_currentUser.Role is UserRole.BranchManager or UserRole.BranchUser)
            throw new UnauthorizedAccessException("API anahtarı yönetimi için yetkiniz yok.");
    }

    private Guid ResolveTenantId()
    {
        if (_currentUser.IsGlobalAdmin)
            return _currentUser.TenantId ?? throw new ArgumentException("Yönetici için TenantId belirtilmedi.");

        return _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");
    }

    private static ApiKeyResponse ToResponse(ApiKey k) => new()
    {
        Id = k.Id,
        TenantId = k.TenantId,
        TenantName = k.Tenant?.Name ?? string.Empty,
        BranchId = k.BranchId,
        BranchName = k.Branch?.Name,
        Name = k.Name,
        KeyPrefix = k.KeyPrefix,
        IsActive = k.IsActive,
        RateLimitPerMinute = k.RateLimitPerMinute,
        RateLimitPerDay = k.RateLimitPerDay,
        AllowedDomains = k.AllowedDomains?.ToList() ?? new List<string>(),
        CreatedAt = k.CreatedAt,
        LastUsedAt = k.LastUsedAt
    };
}
