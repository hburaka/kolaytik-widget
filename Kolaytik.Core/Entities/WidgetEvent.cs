using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Entities;

public class WidgetEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApiKeyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? WidgetConfigId { get; set; }
    public Guid ListId { get; set; }
    public WidgetEventType EventType { get; set; }
    public Guid? SelectedItemId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApiKey ApiKey { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public WidgetConfig? WidgetConfig { get; set; }
    public List List { get; set; } = null!;
    public ListItem? SelectedItem { get; set; }
}
