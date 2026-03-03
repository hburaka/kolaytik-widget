namespace Kolaytik.Blazor.Models.Tenants;

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? SectorId { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string Status { get; set; } = "Active";
}
