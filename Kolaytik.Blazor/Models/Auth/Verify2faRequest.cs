namespace Kolaytik.Blazor.Models.Auth;

public class Verify2faRequest
{
    public string PreAuthToken { get; set; } = string.Empty;
    public string TotpCode { get; set; } = string.Empty;
}
