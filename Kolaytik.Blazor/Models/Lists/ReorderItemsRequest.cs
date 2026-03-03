namespace Kolaytik.Blazor.Models.Lists;

public class ReorderItemsRequest
{
    public List<Guid> ItemIds { get; set; } = new();
}
