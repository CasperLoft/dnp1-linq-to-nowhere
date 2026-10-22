using Entities;
using Microsoft.AspNetCore.Identity;
using RepositoryContracts;

namespace Services;

public class AuthService : IAuthService
{
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUserRepository _userRepository;

    public AuthService(IPasswordHasher<User> passwordHasher, IUserRepository userRepository)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
    }

    public async Task<User?> ValidateUser(string username, string password)
    {
        User? user = await _userRepository.GetByUsernameAsync(username);

        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _userRepository.UpdateAsync(user);
        }

        return user;
    }
}