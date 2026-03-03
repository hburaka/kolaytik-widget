namespace Kolaytik.Blazor.Models.Lists;

public class CreateListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
}
