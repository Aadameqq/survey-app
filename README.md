# Contest app
A contest management platform that enables easy organization of programming competitions, designed to be convenient, easy to develop, and highly customizable.

## Development

### Running app
Run app
```bash
dotnet run --project ./src/Api/Api.csproj
```

Run app in watch mode
```bash
dotnet watch --project ./src/Api/Api.csproj
```

### Other Services

Run all services (db, redis, smtp, etc.)
```bash
docker-compose up
```

### Database

Create migration
```bash
dotnet ef migrations add MigrationName --project ./src/Infrastructure --startup-project ./src/Api
```

Apply migration
```bash
dotnet ef database update --project ./src/Infrastructure --startup-project ./src/Api
```

### Linting

Check code style
```bash
dotnet format style --verify-no-changes
dotnet format analyzers --verify-no-changes
```

Check formatting
```bash
dotnet csharpier . --check
```
