namespace Kolaytik.Blazor.Models.Lists;

public class BulkCreateItemsRequest
{
    public List<CreateListItemRequest> Items { get; set; } = new();
}
