namespace Kolaytik.Core.Entities;

public class Branch : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    public ICollection<List> Lists { get; set; } = new List<List>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
