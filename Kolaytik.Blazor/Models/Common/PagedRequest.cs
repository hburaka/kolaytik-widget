namespace Kolaytik.Blazor.Models.Common;

public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public Guid? BranchId { get; set; }
}
