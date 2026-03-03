namespace Kolaytik.Blazor.Models.Branches;

public class CreateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
}
