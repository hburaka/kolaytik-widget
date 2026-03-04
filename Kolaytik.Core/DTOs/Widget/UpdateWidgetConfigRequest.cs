namespace Kolaytik.Core.DTOs.Widget;

public class UpdateWidgetConfigRequest
{
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public IList<WidgetConfigLevelDto> Levels { get; set; } = new List<WidgetConfigLevelDto>();
}
