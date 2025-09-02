NekoViBE

NekoViBE is an anime/wibu e-commerce backend built with .NET 8. The solution follows a clean layered architecture with CRRS pattern, integrates ASP.NET Core Identity and JWT, supports startup data seeding (roles, admin), and automatically applies EF Core migrations in Development.

Architecture

- src/NekoViBE.API: Web API (.NET 8)
  - DI configuration, middlewares, Swagger, CORS
  - Automatic Migrate/Seed in Development
- src/NekoViBE.Application: Application layer
  - Interfaces, Models, Generic Repository, Unit of Work, Query Builders, Feature (Future) including CQRS pattern
- src/NekoViBE.Domain: Domain layer
  - Entities, Enums, Base types (BaseEntity, IEntityLike)
- src/NekoViBE.Infrastructure: Infrastructure layer
  - EF Core NekoViDbContext + DI, Identity stores, SQL Server provider

Prerequisites

- .NET 8 SDK
- SQL Server (local or container)
- dotnet-ef CLI

Install dotnet-ef (if needed):
```bash
dotnet tool install --global dotnet-ef
```

Configuration

Copy the example settings and adjust to your environment:
```bash
cd src/NekoViBE.API
copy appsettings.example.json appsettings.json
```
Important keys:
- ConnectionStrings:DefaultConnection
- Jwt:Key
- AdminUser (Email, DefaultPassword)
- DataSeeding:EnableSeeding (true to seed roles + admin)
- Cors:AllowedOrigins

Security note: Always change AdminUser.DefaultPassword in your appsettings before running in any non-local environment.

EF Core Migrations

Create a migration (e.g., InitialContext):
```bash
dotnet ef migrations add InitialContext --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

Apply migrations manually (optional):
```bash
dotnet ef database update --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

Note: In Development, the API automatically applies pending migrations at startup (see Program.cs).

Run the project

Run API:
```bash
cd src/NekoViBE.API
dotnet run
```

Watch mode (rebuild on changes):
```bash
cd src/NekoViBE.API
dotnet watch run
```

In Development, on startup the API will:
- Retry DB connectivity
- Apply pending migrations
- Seed data (roles from RoleEnum, admin from appsettings)

Implementation notes

- Identity: AppUser : IdentityUser<Guid>, AppRole : IdentityRole<Guid>; tables are renamed in NekoViDbContext.
- Soft delete and audit for BaseEntity; Identity entities implement IEntityLike to reuse common filters/sorting.
- DbContext pooling is enabled; extra configuration is moved into AddDbContextPool.

Useful commands

- List migrations:
```bash
dotnet ef migrations list --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```
- Remove last migration (not applied):
```bash
dotnet ef migrations remove --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```
- Create/update database manually:
```bash
dotnet ef database update --project src/NekoViBE.Infrastructure --startup-project src/NekoViBE.API
```

Troubleshooting

- SQL Server: verify ConnectionStrings:DefaultConnection and user permissions. With Docker, publish port 1433 and consider TrustServerCertificate=True when using self-signed certs.
