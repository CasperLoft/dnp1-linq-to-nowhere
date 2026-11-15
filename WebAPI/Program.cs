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
using WebAPI.Auth;
using Microsoft.AspNetCore.Authentication;
using Enums;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("Database")
    ?? new SqliteConnectionStringBuilder
    {
        DataSource = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "EfcRepositories", "app.db"))
    }.ToString();

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddAuthentication("Session").AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>("Session", _ => { });
builder.Services.AddAuthorization();

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

if (args.Length > 0 && args[0] == "provision-admin")
{
    if (app.Environment.IsDevelopment() || args.Length != 2 || string.IsNullOrWhiteSpace(args[1]) || Console.IsInputRedirected)
        throw new InvalidOperationException("Use provision-admin <username> outside Development in an interactive terminal.");
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<EfcRepositories.AppContext>();
    if ((await context.Database.GetPendingMigrationsAsync()).Any())
        throw new InvalidOperationException("Apply the database migrations before provisioning an admin.");
    var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    if (await repository.GetByUsernameAsync(args[1]) is not null)
        throw new InvalidOperationException("That username is already in use.");
    Console.Write("Password: ");
    var password = new System.Text.StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter) break;
        if (key.Key == ConsoleKey.Backspace) { if (password.Length > 0) password.Length--; }
        else if (!char.IsControl(key.KeyChar)) password.Append(key.KeyChar);
    }
    Console.WriteLine();
    if (password.Length == 0) throw new InvalidOperationException("Password cannot be empty.");
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    await repository.AddAsync(new User(args[1], hasher.HashPassword(null!, password.ToString()), UserRole.Admin));
    Console.WriteLine("Admin account created.");
    return;
}

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
app.UseAuthentication();
app.UseAuthorization();
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
