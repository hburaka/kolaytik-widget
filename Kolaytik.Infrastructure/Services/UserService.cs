using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.User;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kolaytik.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UserService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<UserResponse>> GetUsersAsync(PagedRequest request)
    {
        var query = BuildUserScope();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(term));
        }

        if (request.BranchId.HasValue)
        {
            var userIds = _db.UserBranches
                .Where(ub => ub.BranchId == request.BranchId.Value)
                .Select(ub => ub.UserId);
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                TenantId = u.TenantId,
                Status = u.Status,
                Is2faEnabled = u.Is2faEnabled,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<UserResponse>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<UserResponse> GetUserAsync(Guid id)
    {
        var user = await BuildUserScope()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        return ToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        AssertCanManageUsers();

        var tenantId = ResolveTenantId(request.TenantId);

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Bu e-mail adresi zaten kayıtlı.");

        if (!_currentUser.IsGlobalAdmin && request.Role is UserRole.SuperAdmin or UserRole.Admin)
            throw new UnauthorizedAccessException("Bu rolde kullanıcı oluşturma yetkiniz yok.");

        var user = new User
        {
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            TenantId = tenantId,
            Status = UserStatus.Active
        };

        await _db.Users.AddAsync(user);

        if (request.BranchId.HasValue && tenantId.HasValue)
        {
            var branchExists = await _db.Branches.AnyAsync(b =>
                b.Id == request.BranchId.Value && b.TenantId == tenantId.Value);
            if (!branchExists)
                throw new ArgumentException("Şube bu firmaya ait değil.");

            await _db.UserBranches.AddAsync(new UserBranch
            {
                UserId = user.Id,
                BranchId = request.BranchId.Value
            });
        }

        await _db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        AssertCanManageUsers();

        var user = await BuildUserScope()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (!_currentUser.IsGlobalAdmin && request.Role is UserRole.SuperAdmin or UserRole.Admin)
            throw new UnauthorizedAccessException("Bu rolü atama yetkiniz yok.");

        user.Role = request.Role;
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        // Şube atamalarını güncelle (BranchUser/BranchManager için)
        if (request.BranchIds.Count > 0 || user.Role is UserRole.BranchManager or UserRole.BranchUser)
        {
            var existing = await _db.UserBranches.Where(ub => ub.UserId == id).ToListAsync();
            _db.UserBranches.RemoveRange(existing);

            if (request.BranchIds.Count > 0)
            {
                var tenantId = user.TenantId;
                if (tenantId.HasValue)
                {
                    var validBranchIds = await _db.Branches
                        .Where(b => request.BranchIds.Contains(b.Id) && b.TenantId == tenantId.Value)
                        .Select(b => b.Id)
                        .ToListAsync();

                    await _db.UserBranches.AddRangeAsync(validBranchIds.Select(branchId => new UserBranch
                    {
                        UserId = id,
                        BranchId = branchId
                    }));
                }
            }
        }

        await _db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        AssertCanManageUsers();

        if (id == _currentUser.UserId)
            throw new InvalidOperationException("Kendi hesabınızı silemezsiniz.");

        var user = await BuildUserScope()
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Yardımcı ─────────────────────────────────────────────────────────

    private IQueryable<User> BuildUserScope()
    {
        var query = _db.Users.AsQueryable();

        if (_currentUser.IsGlobalAdmin)
            return query;

        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");

        query = query.Where(u => u.TenantId == tenantId);

        if (_currentUser.Role == UserRole.BranchManager)
        {
            var branchUserIds = _db.UserBranches
                .Where(ub => _db.UserBranches
                    .Where(x => x.UserId == _currentUser.UserId)
                    .Select(x => x.BranchId)
                    .Contains(ub.BranchId))
                .Select(ub => ub.UserId);

            query = query.Where(u => branchUserIds.Contains(u.Id));
        }

        return query;
    }

    private void AssertCanManageUsers()
    {
        if (_currentUser.Role == UserRole.BranchUser)
            throw new UnauthorizedAccessException("Kullanıcı yönetimi için yetkiniz yok.");
    }

    private Guid? ResolveTenantId(Guid? requestedTenantId)
    {
        if (_currentUser.IsGlobalAdmin)
            return requestedTenantId;

        return _currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant bilgisi eksik.");
    }

    private static UserResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Role = u.Role,
        TenantId = u.TenantId,
        Status = u.Status,
        Is2faEnabled = u.Is2faEnabled,
        LastLoginAt = u.LastLoginAt,
        CreatedAt = u.CreatedAt
    };
}
