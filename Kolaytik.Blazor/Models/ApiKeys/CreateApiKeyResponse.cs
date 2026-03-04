namespace Kolaytik.Blazor.Models.ApiKeys;

public class CreateApiKeyResponse
{
    public ApiKeyResponse ApiKey { get; set; } = null!;
    public string PlainKey { get; set; } = string.Empty;
}
