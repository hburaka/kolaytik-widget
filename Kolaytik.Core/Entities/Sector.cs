namespace Kolaytik.Core.Entities;

public class Sector
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
