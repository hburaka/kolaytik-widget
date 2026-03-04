namespace Kolaytik.Core.DTOs.Widget;

public class CreateWidgetConfigRequest
{
    public Guid? TenantId { get; set; } // SuperAdmin/Admin için zorunlu, TenantAdmin için opsiyonel (kendi tenantı kullanılır)
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public IList<WidgetConfigLevelDto> Levels { get; set; } = new List<WidgetConfigLevelDto>();
}
