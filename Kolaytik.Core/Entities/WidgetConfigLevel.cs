using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Entities;

public class WidgetConfigLevel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WidgetConfigId { get; set; }
    public int OrderIndex { get; set; }
    public Guid ListId { get; set; }
    public WidgetElementType ElementType { get; set; } = WidgetElementType.Dropdown;
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; } = true;
    public int? MaxSelections { get; set; }

    public WidgetConfig WidgetConfig { get; set; } = null!;
    public List List { get; set; } = null!;
}
