# LINQ to Nowhere

A DNP1 assignment project with a Blazor web app, a separate ASP.NET Core Web API, and a command-line interface. The API and CLI use JSON files for local data.

## Requirements

- .NET 10 SDK
- A trusted .NET HTTPS development certificate. If needed, run `dotnet dev-certs https --trust` and accept the operating system prompt.

## Run locally

Run these commands from the repository root. First, create the sample data:

```sh
dotnet run --project CLI -- seed --reset
```

**This command deletes all existing users, posts, and comments in the local `data/` directory before recreating the sample data.** The directory is ignored by Git. The CLI uses the current working directory to locate it, so run the command from the repository root.

The sample users are `Alice` (Admin), `Bob` (Moderator), and `Charlie` (User). Their development password is `password123`; it is for local testing only.

Start the API and WebApp in separate terminals:

```sh
dotnet run --project WebAPI
```

```sh
dotnet run --project WebApp
```

Open the WebApp at https://localhost:7018 and the API's Swagger interface at https://localhost:7295/swagger. The WebApp is configured to call the API at https://localhost:7295.

## Tests

Run the solution's tests from the repository root:

```sh
dotnet test
```

## Assignment status

The API has a login endpoint at `POST /auth/login`. The Blazor login page and authentication state are still being implemented; the development login flow is not production-ready.
