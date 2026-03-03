namespace Kolaytik.Blazor.Models.Lists;

public class UpdateListItemRequest
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
}
