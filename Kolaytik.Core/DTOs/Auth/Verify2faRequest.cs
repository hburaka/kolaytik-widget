namespace Kolaytik.Core.DTOs.Auth;

public class Verify2faRequest
{
    public string PreAuthToken { get; set; } = string.Empty;
    public string TotpCode { get; set; } = string.Empty;
}
