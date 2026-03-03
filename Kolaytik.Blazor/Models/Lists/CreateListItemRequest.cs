namespace Kolaytik.Blazor.Models.Lists;

public class CreateListItemRequest
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int? OrderIndex { get; set; }
}
