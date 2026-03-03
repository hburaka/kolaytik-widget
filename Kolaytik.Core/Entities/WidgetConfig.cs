namespace Kolaytik.Core.Entities;

public class WidgetConfig : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";

    public Tenant Tenant { get; set; } = null!;
    public ICollection<WidgetConfigLevel> Levels { get; set; } = new List<WidgetConfigLevel>();
    public ICollection<WidgetEvent> WidgetEvents { get; set; } = new List<WidgetEvent>();
}
