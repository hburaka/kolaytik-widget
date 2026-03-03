using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Entities;

public class Ticket : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Subject { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public Tenant Tenant { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}
