namespace Kolaytik.Core.DTOs.Sector;

public class SectorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TenantCount { get; set; }
}
