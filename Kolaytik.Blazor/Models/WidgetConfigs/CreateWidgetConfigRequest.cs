namespace Kolaytik.Blazor.Models.WidgetConfigs;

public class CreateWidgetConfigRequest
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public List<WidgetConfigLevelDto> Levels { get; set; } = new();
}
