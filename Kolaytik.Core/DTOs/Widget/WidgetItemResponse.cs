using System.Text.Json;

namespace Kolaytik.Core.DTOs.Widget;

public class WidgetItemResponse
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public JsonDocument? Metadata { get; set; }
    public int OrderIndex { get; set; }
    public bool HasChildren { get; set; }
}
