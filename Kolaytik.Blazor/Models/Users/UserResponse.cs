namespace Kolaytik.Blazor.Models.Users;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public List<Guid> BranchIds { get; set; } = new();
    public List<string> BranchNames { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public bool Is2faEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
