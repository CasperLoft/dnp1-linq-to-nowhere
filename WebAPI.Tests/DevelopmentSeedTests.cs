using DTOs.Auth.Requests;
using Entities;
using Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using DatabaseContext = EfcRepositories.AppContext;

namespace WebAPI.Tests;

public class DevelopmentSeedTests
{
    private sealed class SeedFactory(string databasePath, string environment) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(environment);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DatabaseContext>();
                services.RemoveAll<DbContextOptions<DatabaseContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<DatabaseContext>>();
                services.AddDbContext<DatabaseContext>(options => options.UseSqlite(Connection(databasePath)));
            });
        }
    }

    private static string Connection(string path) => new SqliteConnectionStringBuilder { DataSource = path, Pooling = false }.ToString();
    private static DatabaseContext Context(string path) => new(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite(Connection(path)).Options);

    [Fact]
    public async Task DevelopmentStartupSeedsEmptyDatabaseAndDoesNotRepeat()
    {
        string path = Path.Combine(Path.GetTempPath(), $"dnp-seed-{Guid.NewGuid():N}.db");
        try
        {
            await using (var context = Context(path)) await context.Database.MigrateAsync();
            using (var factory = new SeedFactory(path, "Development"))
            using (var client = factory.CreateClient())
            {
                foreach (string userName in new[] { "Alice", "Bob", "Charlie", "SampleUser01", "SampleUser20" })
                    Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/auth/login", new LoginRequest(userName, "password123"))).StatusCode);
            }
            await using (var context = Context(path))
            {
                Assert.Equal(23, await context.Users.CountAsync());
                Assert.Equal(43, await context.Posts.CountAsync());
                Assert.Equal(89, await context.Comments.CountAsync());
                Assert.Equal(UserRole.Admin, (await context.Users.SingleAsync(u => u.UserName == "Alice")).Role);
                Assert.Equal(UserRole.Moderator, (await context.Users.SingleAsync(u => u.UserName == "Bob")).Role);
                var hasher = new PasswordHasher<User>();
                foreach (var user in await context.Users.ToListAsync())
                    Assert.NotEqual(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(user, user.PasswordHash, "password123"));
                Assert.Equal(20, await context.Users.CountAsync(u => u.UserName.StartsWith("SampleUser") && u.Role == UserRole.User));
                Assert.Empty(await context.Database.SqlQueryRaw<string>("PRAGMA foreign_key_check").ToListAsync());
            }
            using (var factory = new SeedFactory(path, "Development"))
            using (factory.CreateClient()) { }
            await using (var context = Context(path))
            {
                Assert.Equal(23, await context.Users.CountAsync());
                Assert.Equal(43, await context.Posts.CountAsync());
                Assert.Equal(89, await context.Comments.CountAsync());
            }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task DevelopmentStartupLeavesPopulatedDatabaseAlone()
    {
        string path = Path.Combine(Path.GetTempPath(), $"dnp-seed-{Guid.NewGuid():N}.db");
        try
        {
            await using (var context = Context(path))
            {
                await context.Database.MigrateAsync();
                context.Users.Add(new User("Existing", "hash"));
                await context.SaveChangesAsync();
            }
            using (var factory = new SeedFactory(path, "Development"))
            using (factory.CreateClient()) { }
            await using (var context = Context(path))
            {
                Assert.Equal("Existing", (await context.Users.SingleAsync()).UserName);
                Assert.Empty(await context.Posts.ToListAsync());
                Assert.Empty(await context.Comments.ToListAsync());
            }
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task NonDevelopmentStartupDoesNotSeed()
    {
        string path = Path.Combine(Path.GetTempPath(), $"dnp-seed-{Guid.NewGuid():N}.db");
        try
        {
            await using (var context = Context(path)) await context.Database.MigrateAsync();
            using (var factory = new SeedFactory(path, "Testing"))
            using (factory.CreateClient()) { }
            await using (var context = Context(path)) Assert.Empty(await context.Users.ToListAsync());
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void DevelopmentStartupExplainsMissingMigrations()
    {
        string path = Path.Combine(Path.GetTempPath(), $"dnp-seed-{Guid.NewGuid():N}.db");
        try
        {
            using var factory = new SeedFactory(path, "Development");
            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("migrations", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
