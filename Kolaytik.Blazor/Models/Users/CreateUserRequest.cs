namespace Kolaytik.Blazor.Models.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "BranchUser";
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
}
