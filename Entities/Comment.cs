namespace Entities;

public class Comment : IEntity
{
    public int Id { get; set; }
    public string Body { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    
    private Comment() { }
    
    public Comment(string body, int postId, int userId)
    {
        Body = body;
        PostId = postId; 
        UserId = userId;
    }
}
