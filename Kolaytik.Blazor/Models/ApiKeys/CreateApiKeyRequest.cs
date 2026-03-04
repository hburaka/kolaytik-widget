namespace Kolaytik.Blazor.Models.ApiKeys;

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public int RateLimitPerMinute { get; set; } = 60;
    public int RateLimitPerDay { get; set; } = 10000;
    public List<string> AllowedDomains { get; set; } = new();
}
