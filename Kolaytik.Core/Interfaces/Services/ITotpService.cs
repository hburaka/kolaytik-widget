namespace Kolaytik.Core.Interfaces.Services;

public interface ITotpService
{
    string GenerateSecret();
    string GetQrCodeUri(string email, string secret);
    bool ValidateCode(string secret, string code);
}
