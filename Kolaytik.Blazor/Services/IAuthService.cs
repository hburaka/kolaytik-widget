namespace Kolaytik.Blazor.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password, string? totpCode = null);
    Task LogoutAsync();
}
