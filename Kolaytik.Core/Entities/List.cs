namespace Kolaytik.Core.Entities;

public class List : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CreatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Branch? Branch { get; set; }
    public User Creator { get; set; } = null!;
    public ICollection<ListItem> Items { get; set; } = new List<ListItem>();
    public ICollection<WidgetConfigLevel> WidgetConfigLevels { get; set; } = new List<WidgetConfigLevel>();
}
