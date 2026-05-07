---
name: ef
description: "Use when working with Entity Framework Core in this workspace: DbContext, DbSet, entity annotations, relationships, migrations, seeding, Include queries, and applying database updates."
---

# Entity Framework Skill

This skill covers the EF Core workflow for the PrviLabos solution.

Use it when the task involves:
- Adding or adjusting EF-ready model annotations
- Configuring `DbContext` or `DbSet` members
- Working with relationships, eager loading, or query shaping
- Creating, removing, or updating migrations
- Applying migrations to the database
- Seeding data through `OnModelCreating`

## Default Workflow
1. Confirm the active `DbContext` and startup project.
2. Check whether the model project stays POCO-only or needs EF configuration in `DbContext`.
3. Prefer fluent configuration for provider-specific concerns such as decimal precision.
4. Generate or update the migration from the DAL project.
5. Apply the migration with `dotnet ef database update`.
6. Rebuild the solution and verify the result.

## Migration Commands
Use the DAL project as the migration target and the web project as the startup project.

```powershell
cd PrviLabos.DAL
& "C:\Program Files\dotnet\dotnet.exe" ef migrations add Initial --startup-project ..\PrviLabos --context PrviLabosDbContext
& "C:\Program Files\dotnet\dotnet.exe" ef database update --startup-project ..\PrviLabos --context PrviLabosDbContext
```

## Model Rules
- Keep domain entities simple and EF-friendly.
- Use `Id` as the primary key.
- Use `ICollection<T>` for collection navigations.
- Use foreign key properties for explicit relationships.
- Prefer `HasPrecision(18, 2)` in `OnModelCreating` for money values.

## Query Rules
- Use `Include` and `ThenInclude` when views need related data.
- Use `AsNoTracking()` for read-only dashboard queries when tracking is unnecessary.
- Avoid unsupported expression-tree constructs in EF queries.

## Validation
- Run `dotnet build` after EF changes.
- Inspect generated migration files before applying them.
- If `dotnet ef` complains about startup references, add `Microsoft.EntityFrameworkCore.Design` to the startup project and rebuild.
