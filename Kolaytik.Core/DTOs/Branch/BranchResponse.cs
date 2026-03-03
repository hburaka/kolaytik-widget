namespace Kolaytik.Core.DTOs.Branch;

public class BranchResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
