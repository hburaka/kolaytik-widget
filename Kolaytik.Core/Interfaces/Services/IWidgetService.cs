using Kolaytik.Core.DTOs.Widget;

namespace Kolaytik.Core.Interfaces.Services;

public interface IWidgetService
{
    Task<WidgetConfigResponse> GetConfigAsync(string apiKey, Guid configId, string? origin, string ipAddress);
    Task<IList<WidgetItemResponse>> GetItemsAsync(string apiKey, Guid listId, Guid? parentItemId, string? origin, string ipAddress);
}
