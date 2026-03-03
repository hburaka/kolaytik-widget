using System.Text.Json;

namespace Kolaytik.Core.Entities;

public class ListItem : BaseEntity
{
    public Guid ListId { get; set; }
    public Guid TenantId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public JsonDocument? Metadata { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }

    public List List { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ListItemRelation> ParentRelations { get; set; } = new List<ListItemRelation>();
    public ICollection<ListItemRelation> ChildRelations { get; set; } = new List<ListItemRelation>();
}
