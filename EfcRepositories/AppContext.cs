using Entities;
using Microsoft.EntityFrameworkCore;

namespace EfcRepositories;

public class AppContext : DbContext
{
    public AppContext() { }

    public AppContext(DbContextOptions<AppContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=/Users/loft/Documents/Software Engineer/3. Semester/DNP1/repo/dnp1-linq-to-nowhere/EfcRepositories/app.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().Property(user => user.UserName).UseCollation("NOCASE");
        modelBuilder.Entity<User>().HasIndex(user => user.UserName).IsUnique();
    }
}
