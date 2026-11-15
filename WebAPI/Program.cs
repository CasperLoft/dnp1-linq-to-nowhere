using Entities;
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

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();

var app = builder.Build();

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
