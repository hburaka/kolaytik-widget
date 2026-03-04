namespace Kolaytik.Blazor.Models.WidgetConfigs;

public class WidgetConfigResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public List<WidgetConfigLevelDto> Levels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
