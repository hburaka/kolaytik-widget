namespace Kolaytik.Core.DTOs.Branch;

public class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
