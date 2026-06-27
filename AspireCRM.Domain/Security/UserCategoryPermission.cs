using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Security;

public class UserCategoryPermission : BaseEntity
{
    public long UserId { get; set; }
    public long CategoryId { get; set; }
    public Category? Category { get; set; }
    public CategoryPermissionLevel PermissionLevel { get; set; } = CategoryPermissionLevel.None;
}