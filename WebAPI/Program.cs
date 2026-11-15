using Entities;
using DevelopmentData;
using EfcRepositories;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using Services;
using Services.Implementations;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("Database")
    ?? new SqliteConnectionStringBuilder
    {
        DataSource = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "EfcRepositories", "app.db"))
    }.ToString();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddDbContext<EfcRepositories.AppContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();
builder.Services.AddScoped<DevelopmentDataSeeder>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<EfcRepositories.AppContext>();
    try
    {
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            throw new InvalidOperationException("The SQLite database has pending migrations. Apply them before starting the API; see the README.");

        await using var transaction = await context.Database.BeginTransactionAsync();
        if (!await context.Users.AnyAsync() && !await context.Posts.AnyAsync() && !await context.Comments.AnyAsync())
        {
            await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync(includeAdditionalActivity: true);
            await transaction.CommitAsync();
        }
    }
    catch (SqliteException exception)
    {
        throw new InvalidOperationException("Could not initialize the development SQLite database. Apply the migrations and check the database connection; see the README.", exception);
    }
}

app.UseExceptionHandler();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        foreach (string address in app.Urls)
        {
            app.Logger.LogInformation("Swagger: {SwaggerUrl}", $"{address}/swagger");
        }
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Run();

public partial class Program;
