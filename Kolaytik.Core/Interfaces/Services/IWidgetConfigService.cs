using Kolaytik.Core.DTOs.Common;
using Kolaytik.Core.DTOs.Widget;

namespace Kolaytik.Core.Interfaces.Services;

public interface IWidgetConfigService
{
    Task<PagedResult<WidgetConfigManagementResponse>> GetWidgetConfigsAsync(PagedRequest request);
    Task<WidgetConfigManagementResponse> GetWidgetConfigAsync(Guid id);
    Task<WidgetConfigManagementResponse> CreateWidgetConfigAsync(CreateWidgetConfigRequest request);
    Task<WidgetConfigManagementResponse> UpdateWidgetConfigAsync(Guid id, UpdateWidgetConfigRequest request);
    Task DeleteWidgetConfigAsync(Guid id);
}
