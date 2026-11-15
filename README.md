# LINQ to Nowhere

A DNP1 assignment project with a Blazor web app, a separate ASP.NET Core Web API, and a command-line interface. The API uses SQLite; the CLI uses JSON files for local data.

## Requirements

- .NET 10 SDK
- A trusted .NET HTTPS development certificate. If needed, run `dotnet dev-certs https --trust` and accept the operating system prompt.

## Run locally

Run these commands from the repository root. Apply the database migrations before starting the API (install the `dotnet-ef` tool if needed):

```sh
dotnet ef database update --project EfcRepositories --connection "Data Source=EfcRepositories/app.db"
```

The API defaults to `EfcRepositories/app.db`. Override this with the `ConnectionStrings:Database` configuration value (for example, the `ConnectionStrings__Database` environment variable). Apply migrations to the chosen database explicitly; startup does not apply them.

When the API starts in Development, it seeds an entirely empty SQLite database with 23 users, 43 posts, and 89 comments. The sample accounts include `Alice` (Admin), `Bob` (Moderator), `Charlie` (User), and `SampleUser01` through `SampleUser20` (Users). All have the development password `password123`. If any of the three tables already contains data, startup leaves the database untouched. Automatic seeding is disabled outside Development.

The SQLite database is separate from the CLI's JSON data. To create the original sample data for the CLI, run:

```sh
dotnet run --project CLI -- seed --reset
```

**This command deletes all existing users, posts, and comments in the local `data/` directory before recreating the sample data.** The directory is ignored by Git. The CLI uses the current working directory to locate it, so run the command from the repository root.

The CLI sample users are `Alice` (Admin), `Bob` (Moderator), and `Charlie` (User). Their development password is `password123`; it is for local testing only. Seeding does not populate SQLite.

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

The API has a login endpoint at `POST /auth/login`, and the Blazor app maintains a development login state. The login flow is not production-ready.

Post and comment permissions are enforced only in the Blazor interface for this assignment. Browser session data can be modified and API endpoints can be called directly without these checks; do not use this authentication flow for production.
