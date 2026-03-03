using System.Text.Json;

namespace Kolaytik.Core.DTOs.List;

public class ListItemResponse
{
    public Guid Id { get; set; }
    public Guid ListId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public JsonDocument? Metadata { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public bool HasChildren { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
