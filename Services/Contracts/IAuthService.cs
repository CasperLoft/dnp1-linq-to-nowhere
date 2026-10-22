using Entities;

namespace Services;

public interface IAuthService
{
    Task<User?> ValidateUser(string username, string password);
}
