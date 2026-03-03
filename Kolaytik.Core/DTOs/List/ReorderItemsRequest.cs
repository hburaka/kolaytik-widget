namespace Kolaytik.Core.DTOs.List;

public class ReorderItemsRequest
{
    /// <summary>Sıralı item ID listesi. Listedeki pozisyon = yeni OrderIndex.</summary>
    public IList<Guid> ItemIds { get; set; } = new List<Guid>();
}
