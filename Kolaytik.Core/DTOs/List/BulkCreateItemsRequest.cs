namespace Kolaytik.Core.DTOs.List;

public class BulkCreateItemsRequest
{
    public IList<CreateListItemRequest> Items { get; set; } = new List<CreateListItemRequest>();
}
