namespace Kolaytik.Blazor.Models.Branches;

public class BranchResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
