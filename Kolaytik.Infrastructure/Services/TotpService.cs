using Kolaytik.Core.Interfaces.Services;
using OtpNet;

namespace Kolaytik.Infrastructure.Services;

public class TotpService : ITotpService
{
    private const string Issuer = "Kolaytik";

    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GetQrCodeUri(string email, string secret)
    {
        var encodedIssuer = Uri.EscapeDataString(Issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secret}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool ValidateCode(string secret, string code)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.VerifyTotp(code.Trim(), out _, new VerificationWindow(2, 2));
    }
}
