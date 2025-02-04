# Contest app

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

### Database

Run database (you can also set up it locally if you wish)
```bash
docker-compose up database
```

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
