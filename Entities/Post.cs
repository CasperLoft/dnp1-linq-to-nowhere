namespace Entities;

public class Post : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public int UserId { get; set; }
    
    private Post() { }

    public Post(string title, string body, int userId)
    {
        Title = title;
        Body = body;
        UserId = userId;
    }
}
