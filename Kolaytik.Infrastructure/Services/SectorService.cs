using Kolaytik.Core.DTOs.Sector;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class SectorService : ISectorService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SectorService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IList<SectorResponse>> GetSectorsAsync()
    {
        AssertIsGlobalAdmin();

        return await _db.Sectors
            .OrderBy(s => s.Name)
            .Select(s => new SectorResponse
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive,
                TenantCount = s.Tenants.Count
            })
            .ToListAsync();
    }

    public async Task<SectorResponse> GetSectorAsync(Guid id)
    {
        AssertIsGlobalAdmin();

        var sector = await _db.Sectors
            .Include(s => s.Tenants)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Sektör bulunamadı.");

        return ToResponse(sector);
    }

    public async Task<SectorResponse> CreateSectorAsync(CreateSectorRequest request)
    {
        AssertIsGlobalAdmin();

        var sector = new Sector
        {
            Name = request.Name.Trim(),
            IsActive = true
        };

        await _db.Sectors.AddAsync(sector);
        await _db.SaveChangesAsync();

        return ToResponse(sector);
    }

    public async Task<SectorResponse> UpdateSectorAsync(Guid id, UpdateSectorRequest request)
    {
        AssertIsGlobalAdmin();

        var sector = await _db.Sectors
            .Include(s => s.Tenants)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Sektör bulunamadı.");

        sector.Name = request.Name.Trim();
        sector.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
        return ToResponse(sector);
    }

    public async Task DeleteSectorAsync(Guid id)
    {
        AssertIsGlobalAdmin();

        var sector = await _db.Sectors
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Sektör bulunamadı.");

        _db.Sectors.Remove(sector);
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private void AssertIsGlobalAdmin()
    {
        if (!_currentUser.IsGlobalAdmin)
            throw new UnauthorizedAccessException("Bu işlem için SuperAdmin veya Admin yetkisi gereklidir.");
    }

    private static SectorResponse ToResponse(Sector s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        IsActive = s.IsActive,
        TenantCount = s.Tenants.Count
    };
}
