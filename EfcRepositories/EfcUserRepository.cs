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
        await _ctx.Users.AddAsync(user);
        await _ctx.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        if (!(await _ctx.Users.AnyAsync(u => u.Id == user.Id)))
        {
            throw new NotFoundException($"User with {user.Id} not found");
        };

        _ctx.Users.Update(user);
        await _ctx.SaveChangesAsync();

    }

    public async Task DeleteAsync(int userId)
    {
        User? existing = await _ctx.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (existing == null)
        {
            throw new NotFoundException($"User with {userId} not found");
        }

        _ctx.Users.Remove(existing);
        await _ctx.SaveChangesAsync();
    }

    public async Task<User> GetByIdAsync(int userId)
    {
        User? existing = await _ctx.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (existing == null)
        {
            throw new NotFoundException($"User with {userId} not found");
        }

        return existing;
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        User? existing = await _ctx.Users.SingleOrDefaultAsync(u => u.UserName == username);
        if (existing == null)
        {
            throw new NotFoundException($"User with {username} not found");
        }

        return existing;
    }

    public IQueryable<User> GetManyAsync()
    {
        return _ctx.Users.AsQueryable();
    }
}