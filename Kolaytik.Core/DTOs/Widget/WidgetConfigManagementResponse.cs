namespace Kolaytik.Core.DTOs.Widget;

/// <summary>Admin yönetim paneli için widget config response (TenantName ve Levels ile).</summary>
public class WidgetConfigManagementResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Width { get; set; } = "100%";
    public IList<WidgetConfigLevelDto> Levels { get; set; } = new List<WidgetConfigLevelDto>();
    public DateTime CreatedAt { get; set; }
}
