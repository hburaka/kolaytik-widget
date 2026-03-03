namespace Kolaytik.Core.DTOs.List;

public class SetRelationsRequest
{
    /// <summary>Bu parent item'ın child item ID listesi. Mevcut ilişkilerin tamamı bu listeyle değiştirilir.</summary>
    public IList<Guid> ChildItemIds { get; set; } = new List<Guid>();
}
