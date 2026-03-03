namespace Kolaytik.Core.Entities;

public class ApiKey : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int RateLimitPerMinute { get; set; } = 60;
    public int RateLimitPerDay { get; set; } = 10000;
    public string[]? AllowedDomains { get; set; }
    public DateTime? LastUsedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Branch? Branch { get; set; }
    public ICollection<WidgetEvent> WidgetEvents { get; set; } = new List<WidgetEvent>();
}
