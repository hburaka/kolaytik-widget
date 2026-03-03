using System.Text.Json;
using Kolaytik.Core.Entities;
using Kolaytik.Core.Enums;
using Kolaytik.Core.Interfaces.Services;
using Kolaytik.Infrastructure.Data;

namespace Kolaytik.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(Guid? userId, string entityType, Guid entityId, AuditAction action,
        object? oldValues = null, object? newValues = null, string ipAddress = "")
    {
        var log = new AuditLog
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues is null ? null : JsonDocument.Parse(JsonSerializer.Serialize(oldValues)),
            NewValues = newValues is null ? null : JsonDocument.Parse(JsonSerializer.Serialize(newValues)),
            IpAddress = ipAddress
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
