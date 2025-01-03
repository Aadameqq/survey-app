# Survey app

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
dotnet ef migrations add MigrationName
```

Apply migration
```bash
dotnet ef database update
```

### Linting
Format

```bash
dotnet format
```

Check
```bash
dotnet format --verify-no-changes
```
