using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Tenant;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TenantService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TenantResponse>> GetTenantsAsync(PagedRequest request)
    {
        AssertIsGlobalAdmin();

        var query = _db.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                SectorName = t.Sector != null ? t.Sector.Name : null,
                TaxNumber = t.TaxNumber,
                AuthorizedName = t.AuthorizedName,
                Phone = t.Phone,
                Email = t.Email,
                Address = t.Address,
                Status = t.Status,
                UserCount = t.Users.Count,
                BranchCount = t.Branches.Count,
                ListCount = t.Lists.Count,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<TenantResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<TenantResponse> GetTenantAsync(Guid id)
    {
        AssertIsGlobalAdmin();

        var tenant = await _db.Tenants
            .Include(t => t.Sector)
            .Include(t => t.Users)
            .Include(t => t.Branches)
            .Include(t => t.Lists)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Firma bulunamadı.");

        return ToResponse(tenant);
    }

    public async Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request, Guid createdBy)
    {
        AssertIsGlobalAdmin();

        var tenant = new Tenant
        {
            Name = request.Name.Trim(),
            SectorId = request.SectorId ?? Guid.Empty,
            TaxNumber = request.TaxNumber?.Trim(),
            AuthorizedName = request.AuthorizedName?.Trim(),
            Phone = request.Phone?.Trim(),
            Email = request.Email?.Trim(),
            Address = request.Address?.Trim(),
            Status = TenantStatus.Active
        };

        await _db.Tenants.AddAsync(tenant);
        await _db.SaveChangesAsync();

        // Reload with sector if SectorId is set
        if (tenant.SectorId != Guid.Empty)
        {
            await _db.Entry(tenant).Reference(t => t.Sector).LoadAsync();
        }

        return ToResponse(tenant);
    }

    public async Task<TenantResponse> UpdateTenantAsync(Guid id, UpdateTenantRequest request)
    {
        AssertIsGlobalAdmin();

        var tenant = await _db.Tenants
            .Include(t => t.Sector)
            .Include(t => t.Users)
            .Include(t => t.Branches)
            .Include(t => t.Lists)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Firma bulunamadı.");

        tenant.Name = request.Name.Trim();
        tenant.SectorId = request.SectorId ?? Guid.Empty;
        tenant.TaxNumber = request.TaxNumber?.Trim();
        tenant.AuthorizedName = request.AuthorizedName?.Trim();
        tenant.Phone = request.Phone?.Trim();
        tenant.Email = request.Email?.Trim();
        tenant.Address = request.Address?.Trim();
        tenant.Status = request.Status;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Reload sector if changed
        if (request.SectorId.HasValue)
        {
            await _db.Entry(tenant).Reference(t => t.Sector).LoadAsync();
        }

        return ToResponse(tenant);
    }

    public async Task DeleteTenantAsync(Guid id)
    {
        AssertIsGlobalAdmin();

        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Firma bulunamadı.");

        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;
        tenant.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private void AssertIsGlobalAdmin()
    {
        if (!_currentUser.IsGlobalAdmin)
            throw new UnauthorizedAccessException("Bu işlem için SuperAdmin veya Admin yetkisi gereklidir.");
    }

    private static TenantResponse ToResponse(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        SectorName = t.Sector?.Name,
        TaxNumber = t.TaxNumber,
        AuthorizedName = t.AuthorizedName,
        Phone = t.Phone,
        Email = t.Email,
        Address = t.Address,
        Status = t.Status,
        UserCount = t.Users.Count,
        BranchCount = t.Branches.Count,
        ListCount = t.Lists.Count,
        CreatedAt = t.CreatedAt
    };
}
