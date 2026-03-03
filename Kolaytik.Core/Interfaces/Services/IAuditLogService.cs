using Kolaytik.Core.Enums;

namespace Kolaytik.Core.Interfaces.Services;

public interface IAuditLogService
{
    Task LogAsync(Guid? userId, string entityType, Guid entityId, AuditAction action,
        object? oldValues = null, object? newValues = null, string ipAddress = "");
}
