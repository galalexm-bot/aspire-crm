namespace AspireCRM.Web.Models;

public class CommentDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public long AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}
