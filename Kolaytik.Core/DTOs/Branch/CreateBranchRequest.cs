namespace Kolaytik.Core.DTOs.Branch;

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
}
