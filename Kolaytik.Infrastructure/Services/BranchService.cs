using Kolaytik.Core.DTOs.Branch;
using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BranchService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<BranchResponse>> GetBranchesAsync(PagedRequest request)
    {
        var query = BuildBranchScope();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BranchResponse
            {
                Id = b.Id,
                TenantId = b.TenantId,
                Name = b.Name,
                IsActive = b.IsActive,
                UserCount = b.UserBranches.Count,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<BranchResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<BranchResponse> GetBranchAsync(Guid id)
    {
        var branch = await BuildBranchScope()
            .Include(b => b.UserBranches)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Şube bulunamadı.");

        return ToResponse(branch);
    }

    public async Task<BranchResponse> CreateBranchAsync(CreateBranchRequest request)
    {
        AssertCanManageBranches();

        var tenantId = ResolveTenantId(request.TenantId);

        var branch = new Branch
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            IsActive = request.IsActive
        };

        await _db.Branches.AddAsync(branch);
        await _db.SaveChangesAsync();
        return ToResponse(branch);
    }

    public async Task<BranchResponse> UpdateBranchAsync(Guid id, UpdateBranchRequest request)
    {
        AssertCanManageBranches();

        var branch = await BuildBranchScope()
            .Include(b => b.UserBranches)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Şube bulunamadı.");

        branch.Name = request.Name.Trim();
        branch.IsActive = request.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToResponse(branch);
    }

    public async Task DeleteBranchAsync(Guid id)
    {
        AssertCanManageBranches();

        var branch = await BuildBranchScope()
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Şube bulunamadı.");

        branch.IsDeleted = true;
        branch.DeletedAt = DateTime.UtcNow;
        branch.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private IQueryable<Branch> BuildBranchScope()
    {
        var query = _db.Branches.AsQueryable();

        if (_currentUser.IsGlobalAdmin)
            return query;

        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

        return query.Where(b => b.TenantId == tenantId);
    }

    private void AssertCanManageBranches()
    {
        if (_currentUser.Role is UserRole.BranchManager or UserRole.BranchUser)
            throw new UnauthorizedAccessException("Şube yönetimi için yetkiniz yok.");
    }

    private Guid ResolveTenantId(Guid? requestedTenantId)
    {
        if (_currentUser.IsGlobalAdmin)
            return requestedTenantId ?? throw new ArgumentException("TenantId zorunludur.");

        return _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");
    }

    private static BranchResponse ToResponse(Branch b) => new()
    {
        Id = b.Id,
        TenantId = b.TenantId,
        Name = b.Name,
        IsActive = b.IsActive,
        UserCount = b.UserBranches.Count,
        CreatedAt = b.CreatedAt
    };
}
