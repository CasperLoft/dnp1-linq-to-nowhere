using Enums;

namespace Entities;

public class User : IEntity
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    
    private User() { }

    public User(string userName, string passwordHash, UserRole role = UserRole.User)
    {
        UserName = userName;
        PasswordHash = passwordHash;
        Role = role;
    }
}
