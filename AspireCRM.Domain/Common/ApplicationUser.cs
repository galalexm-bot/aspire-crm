using Microsoft.AspNetCore.Identity;

namespace AspireCRM.Domain.Common;

public class ApplicationUser : IdentityUser<long>
{
    public long TenantId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}