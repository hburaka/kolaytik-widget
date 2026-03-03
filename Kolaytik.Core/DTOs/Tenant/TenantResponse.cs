using Kolaytik.Core.Enums;

namespace Kolaytik.Core.DTOs.Tenant;

public class TenantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SectorName { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public TenantStatus Status { get; set; }
    public int UserCount { get; set; }
    public int BranchCount { get; set; }
    public int ListCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
