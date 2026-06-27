using AspireCRM.Domain.Security;

namespace AspireCRM.Web.Models;

public class SetPermissionRequest
{
    public long UserId { get; set; }
    public long CategoryId { get; set; }
    public CategoryPermissionLevel PermissionLevel { get; set; }
}

public class UserInfo
{
    public long Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public string DisplayName => $"{FirstName ?? ""} {LastName ?? ""} ({Email})".Trim();
}