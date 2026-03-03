namespace Kolaytik.Blazor.Models.Auth;

public class TwoFactorSetupResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}
