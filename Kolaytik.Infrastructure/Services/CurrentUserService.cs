using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Kolaytik.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; }
    public Guid? TenantId { get; }
    public UserRole Role { get; }

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User
            ?? throw new InvalidOperationException("HTTP context kullanıcısı bulunamadı.");

        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
               ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("Oturum bilgisi geçersiz.");

        UserId = Guid.Parse(sub);

        var tenantClaim = user.FindFirstValue("tenant_id");
        TenantId = tenantClaim is not null ? Guid.Parse(tenantClaim) : null;

        var roleClaim = user.FindFirstValue(ClaimTypes.Role)
                     ?? throw new UnauthorizedAccessException("Rol bilgisi bulunamadı.");

        Role = Enum.Parse<UserRole>(roleClaim);
    }
}
