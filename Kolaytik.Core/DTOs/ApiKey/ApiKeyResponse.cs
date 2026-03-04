namespace Kolaytik.Core.DTOs.ApiKey;

public class ApiKeyResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int RateLimitPerMinute { get; set; }
    public int RateLimitPerDay { get; set; }
    public List<string> AllowedDomains { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
}
