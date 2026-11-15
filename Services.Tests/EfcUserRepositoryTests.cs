using EfcRepositories;
using Entities;
using Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RepositoryContracts.Exceptions;
using AppContext = EfcRepositories.AppContext;

namespace Services.Tests;

public class EfcUserRepositoryTests
{
    private static async Task<(SqliteConnection Connection, AppContext Context)> CreateDatabaseAsync(bool migrate = true)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppContext>().UseSqlite(connection).Options;
        var context = new AppContext(options);
        if (migrate)
        {
            await context.Database.MigrateAsync();
        }
        return (connection, context);
    }

    [Fact]
    public async Task ReadsAreUntrackedAndMissingUsersFollowTheirContracts()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using (connection)
        await using (context)
        {
            var repository = new EfcUserRepository(context);
            var added = await repository.AddAsync(new User("Ada", "hash"));
            Assert.True(added.Id > 0);
            context.ChangeTracker.Clear();

            Assert.Equal(added.Id, (await repository.GetByIdAsync(added.Id)).Id);
            Assert.Equal(added.Id, (await repository.GetByUsernameAsync("ada"))!.Id);
            Assert.Single(await repository.GetManyAsync().ToListAsync());
            Assert.Empty(context.ChangeTracker.Entries());
            Assert.Null(await repository.GetByUsernameAsync("missing"));
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => repository.GetByIdAsync(999));
            Assert.Contains("id 999", exception.Message);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateCopiesCompleteUserForTrackedAndDetachedUsers(bool tracked)
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using (connection)
        await using (context)
        {
            var repository = new EfcUserRepository(context);
            var original = await repository.AddAsync(new User("Ada", "old-hash"));
            int id = original.Id;
            if (!tracked) context.ChangeTracker.Clear();
            var update = tracked ? original : new User("Ada", "old-hash") { Id = id };
            update.UserName = "Grace";
            update.PasswordHash = "rehash";
            update.Role = UserRole.Admin;
            await repository.UpdateAsync(update);
            context.ChangeTracker.Clear();
            var result = await repository.GetByIdAsync(id);
            Assert.Equal("Grace", result.UserName);
            Assert.Equal("rehash", result.PasswordHash);
            Assert.Equal(UserRole.Admin, result.Role);
            await Assert.ThrowsAsync<NotFoundException>(() => repository.UpdateAsync(new User("missing", "hash") { Id = 999 }));
        }
    }

    [Fact]
    public async Task DeleteRemovesUserAndReportsMissingId()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using (connection)
        await using (context)
        {
            var repository = new EfcUserRepository(context);
            var user = await repository.AddAsync(new User("Ada", "hash"));
            await repository.DeleteAsync(user.Id);
            Assert.Null(await repository.GetByUsernameAsync("Ada"));
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => repository.DeleteAsync(user.Id));
            Assert.Contains($"id {user.Id}", exception.Message);
        }
    }

    [Fact]
    public async Task UniqueIndexRejectsCaseVariants()
    {
        var (connection, context) = await CreateDatabaseAsync();
        await using (connection)
        await using (context)
        {
            var repository = new EfcUserRepository(context);
            await repository.AddAsync(new User("Ada", "hash"));
            await Assert.ThrowsAsync<DbUpdateException>(() => repository.AddAsync(new User("ada", "hash")));
        }
    }

    [Fact]
    public async Task MigrationPreservesCleanExistingData()
    {
        var (connection, context) = await CreateDatabaseAsync(false);
        await using (connection)
        await using (context)
        {
            await context.GetService<IMigrator>().MigrateAsync("20261105103514_initial");
            await context.Database.ExecuteSqlRawAsync("INSERT INTO Users (UserName, PasswordHash, Role) VALUES ('Ada', 'hash', 0)");
            await context.Database.MigrateAsync();
            Assert.Equal("Ada", (await new EfcUserRepository(context).GetByUsernameAsync("ada"))!.UserName);
        }
    }

    [Fact]
    public async Task MigrationFailsWithoutDeletingDuplicateUsers()
    {
        var (connection, context) = await CreateDatabaseAsync(false);
        await using (connection)
        await using (context)
        {
            await context.GetService<IMigrator>().MigrateAsync("20261105103514_initial");
            await context.Database.ExecuteSqlRawAsync("INSERT INTO Users (UserName, PasswordHash, Role) VALUES ('Ada', 'first', 0), ('ada', 'second', 0)");
            await Assert.ThrowsAsync<SqliteException>(() => context.Database.MigrateAsync());
            Assert.Equal(2, await context.Users.CountAsync());
            Assert.Single(await context.Database.GetAppliedMigrationsAsync());
        }
    }
}
