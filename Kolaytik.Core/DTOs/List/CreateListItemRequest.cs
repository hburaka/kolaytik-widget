using System.Text.Json;

namespace Kolaytik.Core.DTOs.List;

public class CreateListItemRequest
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public JsonDocument? Metadata { get; set; }
    public int? OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
}
