using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using RepositoryContracts.Exceptions;

namespace EfcRepositories;

public class EfcUserRepository : IUserRepository
{
    private readonly AppContext _ctx;

    public EfcUserRepository(AppContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<User> AddAsync(User user)
    {
        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        User? existing = await _ctx.Users.FindAsync(user.Id);
        if (existing is null)
        {
            throw new NotFoundException($"User with id {user.Id} not found");
        }

        existing.UserName = user.UserName;
        existing.PasswordHash = user.PasswordHash;
        existing.Role = user.Role;
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        User? existing = await _ctx.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (existing is null)
        {
            throw new NotFoundException($"User with id {userId} not found");
        }

        _ctx.Users.Remove(existing);
        await _ctx.SaveChangesAsync();
    }

    public async Task<User> GetByIdAsync(int userId)
    {
        User? existing = await _ctx.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
        if (existing is null)
        {
            throw new NotFoundException($"User with id {userId} not found");
        }

        return existing;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _ctx.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserName == username);
    }

    public IQueryable<User> GetManyAsync()
    {
        return _ctx.Users.AsNoTracking();
    }
}
