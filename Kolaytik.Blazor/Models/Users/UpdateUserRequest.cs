namespace Kolaytik.Blazor.Models.Users;

public class UpdateUserRequest
{
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<Guid> BranchIds { get; set; } = new();
}
