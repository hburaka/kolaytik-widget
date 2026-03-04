namespace Kolaytik.Core.DTOs.Widget;

public class WidgetConfigLevelDto
{
    public Guid? Id { get; set; } // null = yeni level
    public int OrderIndex { get; set; }
    public Guid ListId { get; set; }
    public string ListName { get; set; } = string.Empty;
    public string ElementType { get; set; } = "Dropdown"; // "Dropdown","RadioButton","CheckboxGroup","MultiSelectDropdown"
    public string Label { get; set; } = string.Empty;
    public string? Placeholder { get; set; }
    public bool IsRequired { get; set; } = true;
    public int? MaxSelections { get; set; }
}
