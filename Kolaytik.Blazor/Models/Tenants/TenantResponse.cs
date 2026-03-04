namespace Kolaytik.Blazor.Models.Tenants;

public class TenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? SectorId { get; set; }
    public string? SectorName { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int BranchCount { get; set; }
    public int ListCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
