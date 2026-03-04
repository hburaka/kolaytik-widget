using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Kolaytik.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly IMemoryCache _cache;

    // SuperAdmin ve Admin için daha kısa token süresi (4 saat)
    private static readonly HashSet<UserRole> ShortLivedRoles = [UserRole.SuperAdmin, UserRole.Admin];

    public TokenService(IConfiguration config, IMemoryCache cache)
    {
        _config = config;
        _cache = cache;
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public string GenerateAccessToken(User user)
    {
        var hours = ShortLivedRoles.Contains(user.Role)
            ? _config.GetValue<int>("Jwt:AdminTokenExpirationHours", 4)
            : _config.GetValue<int>("Jwt:UserTokenExpirationHours", 8);

        var claims = BuildUserClaims(user);
        return CreateJwt(claims, TimeSpan.FromHours(hours));
    }

    public string GeneratePreAuthToken(Guid userId)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        _cache.Set($"preauth:{token}", userId, TimeSpan.FromMinutes(5));
        return token;
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token, requirePreAuth: false);
        var sub = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public Guid? GetUserIdFromPreAuthToken(string token)
    {
        if (_cache.TryGetValue($"preauth:{token}", out Guid userId))
        {
            _cache.Remove($"preauth:{token}"); // tek kullanım
            return userId;
        }
        return null;
    }

    // ── private helpers ──────────────────────────────────────────────────────

    private static Claim[] BuildUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.TenantId.HasValue)
            claims.Add(new Claim("tenant_id", user.TenantId.Value.ToString()));

        return claims.ToArray();
    }

    private string CreateJwt(IEnumerable<Claim> claims, TimeSpan expiry)
    {
        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal? ValidateToken(string token, bool requirePreAuth)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = _signingKey,
                ClockSkew = TimeSpan.Zero
            }, out _);

            if (requirePreAuth && principal.FindFirstValue("pre_auth") != "true")
                return null;

            if (!requirePreAuth && principal.FindFirstValue("pre_auth") == "true")
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
