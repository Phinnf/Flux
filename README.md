dotnet ef database update --context FluxDbContext
# Create a new migration. Replace <Migration_Name> with a meaningful name.
dotnet ef migrations add <Migration_Name> --context FluxDbContext --output-dir Migrations
