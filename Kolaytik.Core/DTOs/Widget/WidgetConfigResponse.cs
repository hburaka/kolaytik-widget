using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.Widget;

public class WidgetConfigResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public IList<WidgetConfigLevelResponse> Levels { get; set; } = new List<WidgetConfigLevelResponse>();
}

public class WidgetConfigLevelResponse
{
    public int OrderIndex { get; set; }
    public Guid ListId { get; set; }
    public WidgetElementType ElementType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; }
    public int? MaxSelections { get; set; }
}
