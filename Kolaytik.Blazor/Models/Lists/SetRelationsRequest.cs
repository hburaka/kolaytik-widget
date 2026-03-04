namespace Kolaytik.Blazor.Models.Lists;

public class SetRelationsRequest
{
    public List<Guid> ChildItemIds { get; set; } = new();
}
