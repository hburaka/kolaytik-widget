using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? SectorId { get; set; }
    public string? TaxNumber { get; set; }
    public string? Address { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public Sector? Sector { get; set; }
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<List> Lists { get; set; } = new List<List>();
    public ICollection<WidgetConfig> WidgetConfigs { get; set; } = new List<WidgetConfig>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
