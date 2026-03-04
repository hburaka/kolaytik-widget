namespace Kolaytik.Core.DTOs.ApiKey;

public class UpdateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RateLimitPerMinute { get; set; }
    public int RateLimitPerDay { get; set; }
    public List<string> AllowedDomains { get; set; } = new();
}
