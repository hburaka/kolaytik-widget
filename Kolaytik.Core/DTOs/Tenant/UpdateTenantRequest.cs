using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.Tenant;

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? SectorId { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public TenantStatus Status { get; set; }
}
