using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace WebAPI.Auth;

public sealed class SessionStore(TimeProvider clock)
{
    private readonly ConcurrentDictionary<string, (int UserId, DateTimeOffset Expires)> sessions = new();

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public string Create(int userId)
    {
        foreach (var session in sessions)
            if (session.Value.Expires <= clock.GetUtcNow()) sessions.TryRemove(session.Key, out _);
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        sessions[Hash(token)] = (userId, clock.GetUtcNow().AddHours(8));
        return token;
    }

    public int? GetUserId(string token)
    {
        string key = Hash(token);
        if (!sessions.TryGetValue(key, out var session)) return null;
        if (session.Expires > clock.GetUtcNow()) return session.UserId;
        sessions.TryRemove(key, out _);
        return null;
    }

    public void Remove(string token) => sessions.TryRemove(Hash(token), out _);

    public void RemoveForUser(int userId)
    {
        foreach (var session in sessions)
            if (session.Value.UserId == userId) sessions.TryRemove(session.Key, out _);
    }
}
