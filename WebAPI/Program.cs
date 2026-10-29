using Entities;
using FileRepositories;
using Microsoft.AspNetCore.Identity;
using RepositoryContracts;
using Services;
using Services.Implementations;
using WebAPI;

var builder = WebApplication.CreateBuilder(args);
string dataDirectory = builder.Configuration["DataDirectory"]
    ?? Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "data"));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPostRepository>(_ => new PostFileRepository(dataDirectory));
builder.Services.AddScoped<IUserRepository>(_ => new UserFileRepository(dataDirectory));
builder.Services.AddScoped<ICommentRepository>(_ => new CommentFileRepository(dataDirectory));

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
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Run();

public partial class Program;
