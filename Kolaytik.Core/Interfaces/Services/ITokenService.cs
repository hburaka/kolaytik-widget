using Kolaytik.Core.Entities;

namespace Kolaytik.Core.Interfaces.Services;

public interface ITokenService
{
    /// <summary>Rol bazlı süreli JWT access token üretir.</summary>
    string GenerateAccessToken(User user);

    /// <summary>2FA adımı için kısa süreli (5 dk) pre-auth token üretir.</summary>
    string GeneratePreAuthToken(Guid userId);

    /// <summary>Güvenli rastgele refresh token döner (plain text — hash'i DB'ye yazılır).</summary>
    string GenerateRefreshToken();

    /// <summary>Token'dan UserId okur; geçersizse null.</summary>
    Guid? GetUserIdFromToken(string token);

    /// <summary>Pre-auth token'dan UserId okur; claim yoksa veya süresi bittiyse null.</summary>
    Guid? GetUserIdFromPreAuthToken(string token);

    /// <summary>Refresh token'ı SHA-256 ile hash'ler.</summary>
    string HashRefreshToken(string token);
}
