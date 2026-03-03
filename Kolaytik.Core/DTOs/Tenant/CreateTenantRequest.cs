namespace Kolaytik.Core.DTOs.Tenant;

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? SectorId { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
