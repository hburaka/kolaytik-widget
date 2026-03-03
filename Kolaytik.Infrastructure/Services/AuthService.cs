using Kolaytik.Core.DTOs.Auth;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using BC = BCrypt.Net.BCrypt;

namespace Kolaytik.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ITotpService _totpService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;

    // Roller için 2FA zorunlu
    private static readonly HashSet<UserRole> RequiredTwoFaRoles = [UserRole.SuperAdmin, UserRole.Admin];
    private const string RefreshTokenPurpose = "refresh";

    public AuthService(
        ApplicationDbContext db,
        ITokenService tokenService,
        ITotpService totpService,
        IMemoryCache cache,
        IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _totpService = totpService;
        _cache = cache;
        _config = config;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Rate limiting kontrolü
        ThrowIfLockedOut(email);

        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BC.Verify(request.Password, user.PasswordHash))
        {
            RecordFailedAttempt(email);
            throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");
        }

        // Başarılı giriş → sayacı sıfırla
        ClearFailedAttempts(email);

        // Kullanıcı durumu kontrolü
        if (user.Status == UserStatus.EmailNotVerified)
            throw new UnauthorizedAccessException("E-posta adresinizi doğrulamanız gerekiyor.");

        if (user.Status == UserStatus.Passive)
            throw new UnauthorizedAccessException("Hesabınız pasif durumda. Yöneticinizle iletişime geçin.");

        // Tenant durumu kontrolü
        if (user.Tenant is not null && user.Tenant.Status != TenantStatus.Active)
            throw new UnauthorizedAccessException("Firmanızın hesabı askıya alınmış. Destek ekibiyle iletişime geçin.");

        // 2FA gerekli mi?
        bool requires2fa = RequiredTwoFaRoles.Contains(user.Role)
                           || user.Is2faEnabled;

        if (requires2fa)
        {
            // TOTP kodu bu istekte gönderilmemişse pre-auth token ver
            if (string.IsNullOrWhiteSpace(request.TotpCode))
            {
                var preAuthToken = _tokenService.GeneratePreAuthToken(user.Id);
                return new LoginResponse
                {
                    Requires2fa = true,
                    PreAuthToken = preAuthToken,
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role
                };
            }

            // TOTP kodu varsa burada doğrula
            if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
                throw new InvalidOperationException("2FA henüz yapılandırılmamış. Lütfen yöneticinizle iletişime geçin.");

            if (!_totpService.ValidateCode(user.TwoFactorSecret, request.TotpCode))
                throw new UnauthorizedAccessException("Geçersiz doğrulama kodu.");
        }

        return await IssueTokensAsync(user, ipAddress);
    }

    public async Task<LoginResponse> Verify2faAsync(Verify2faRequest request, string ipAddress)
    {
        var userId = _tokenService.GetUserIdFromPreAuthToken(request.PreAuthToken)
            ?? throw new UnauthorizedAccessException("Geçersiz veya süresi dolmuş oturum.");

        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            throw new InvalidOperationException("2FA henüz yapılandırılmamış.");

        if (!_totpService.ValidateCode(user.TwoFactorSecret, request.TotpCode))
            throw new UnauthorizedAccessException("Geçersiz doğrulama kodu.");

        return await IssueTokensAsync(user, ipAddress);
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var stored = await _db.RefreshTokens
            .Include(r => r.User).ThenInclude(u => u.Tenant)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash)
            ?? throw new UnauthorizedAccessException("Geçersiz refresh token.");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Refresh token süresi dolmuş veya iptal edilmiş.");

        // Rotate: eskiyi iptal et, yenisini ver
        stored.RevokedAt = DateTime.UtcNow;
        _db.RefreshTokens.Update(stored);

        return await IssueTokensAsync(stored.User, ipAddress);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, Guid userId)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);

        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && r.UserId == userId);

        if (stored is null || !stored.IsActive) return;

        stored.RevokedAt = DateTime.UtcNow;
        _db.RefreshTokens.Update(stored);
        await _db.SaveChangesAsync();
    }

    public async Task<TwoFactorSetupResponse> Setup2faAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var secret = _totpService.GenerateSecret();
        user.TwoFactorSecret = secret;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new TwoFactorSetupResponse
        {
            Secret = secret,
            QrCodeUri = _totpService.GetQrCodeUri(user.Email, secret)
        };
    }

    public async Task<bool> Confirm2faAsync(Guid userId, string totpCode)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            return false;

        if (!_totpService.ValidateCode(user.TwoFactorSecret, totpCode))
            return false;

        user.Is2faEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task Disable2faAsync(Guid userId, string totpCode)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (RequiredTwoFaRoles.Contains(user.Role))
            throw new InvalidOperationException("Bu rol için 2FA devre dışı bırakılamaz.");

        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret) || !_totpService.ValidateCode(user.TwoFactorSecret, totpCode))
            throw new UnauthorizedAccessException("Geçersiz doğrulama kodu.");

        user.Is2faEnabled = false;
        user.TwoFactorSecret = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Mevcut şifre hatalı.");

        user.PasswordHash = BC.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Tüm refresh token'ları iptal et (güvenlik)
        var tokens = await _db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var t in tokens)
            t.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private async Task<LoginResponse> IssueTokensAsync(User user, string ipAddress)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var tokenHash = _tokenService.HashRefreshToken(rawRefreshToken);

        var refreshDays = _config.GetValue<int>("Jwt:RefreshTokenExpirationDays", 30);
        var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

        var stored = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = refreshExpiry,
            IpAddress = ipAddress
        };

        await _db.RefreshTokens.AddAsync(stored);

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Token süresi: rol bazlı hesapla
        var hours = (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin)
            ? _config.GetValue<int>("Jwt:AdminTokenExpirationHours", 4)
            : _config.GetValue<int>("Jwt:UserTokenExpirationHours", 8);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(hours),
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role,
            Requires2fa = false
        };
    }

    // Rate limiting (IMemoryCache tabanlı)
    private void ThrowIfLockedOut(string email)
    {
        if (_cache.TryGetValue(LockoutKey(email), out DateTime lockedUntil) && DateTime.UtcNow < lockedUntil)
        {
            var remaining = (int)(lockedUntil - DateTime.UtcNow).TotalSeconds;
            throw new UnauthorizedAccessException($"Çok fazla başarısız giriş. {remaining} saniye bekleyin.");
        }
    }

    private void RecordFailedAttempt(string email)
    {
        var maxAttempts = _config.GetValue<int>("RateLimiting:LoginMaxAttempts", 5);
        var lockoutMinutes = _config.GetValue<int>("RateLimiting:LoginLockoutMinutes", 5);
        var attemptsKey = AttemptsKey(email);

        var attempts = _cache.GetOrCreate(attemptsKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(lockoutMinutes);
            return 0;
        });

        attempts++;
        _cache.Set(attemptsKey, attempts, TimeSpan.FromMinutes(lockoutMinutes));

        if (attempts >= maxAttempts)
        {
            var lockUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
            _cache.Set(LockoutKey(email), lockUntil, TimeSpan.FromMinutes(lockoutMinutes));
            _cache.Remove(attemptsKey);
            throw new UnauthorizedAccessException($"Çok fazla başarısız giriş. {lockoutMinutes} dakika bekleyin.");
        }
    }

    private void ClearFailedAttempts(string email)
    {
        _cache.Remove(AttemptsKey(email));
        _cache.Remove(LockoutKey(email));
    }

    private static string AttemptsKey(string email) => $"login_attempts:{email}";
    private static string LockoutKey(string email) => $"login_lockout:{email}";
}
