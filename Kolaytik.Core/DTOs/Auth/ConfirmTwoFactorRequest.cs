namespace Kolaytik.Core.DTOs.Auth;

public class ConfirmTwoFactorRequest
{
    public string TotpCode { get; set; } = string.Empty;
}
