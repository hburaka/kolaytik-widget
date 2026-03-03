namespace Kolaytik.Core.Entities;

public class ListItemRelation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ParentItemId { get; set; }
    public Guid ChildItemId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ListItem ParentItem { get; set; } = null!;
    public ListItem ChildItem { get; set; } = null!;
}
