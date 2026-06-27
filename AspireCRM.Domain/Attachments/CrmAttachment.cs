using AspireCRM.Domain.Common;

namespace AspireCRM.Domain.Attachments;

public class CrmAttachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public long EntityId { get; set; }
}
