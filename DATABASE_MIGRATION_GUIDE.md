# Database Migration Guide

This guide explains how to apply the Category table migration on your VPS.

## Migration: AddCategoryTable

This migration:
1. Creates the `Categories` table
2. Migrates existing category strings from `Links.Category` to the new `Categories` table
3. Adds `CategoryId` foreign key column to `Links` table
4. Updates all links to reference the new category IDs
5. Removes the old `Category` string column from `Links`

---

## Option 1: Generate SQL Script Locally (Recommended)

This is the safest approach - generate the SQL on your local machine, review it, then run on VPS.

### Step 1: Generate SQL script locally

```bash
cd D:/side-project/links-backend/LinksService
dotnet ef migrations script --idempotent -o migration.sql
```

The `--idempotent` flag makes the script safe to run multiple times (it checks if migrations are already applied).

### Step 2: Copy script to VPS

```bash
scp migration.sql user@your-vps:/path/to/migration.sql
```

### Step 3: Run on VPS

```bash
mysql -u your_mysql_user -p your_database_name < migration.sql
```

Or if you prefer to run interactively:

```bash
mysql -u your_mysql_user -p your_database_name
mysql> source /path/to/migration.sql;
```

---

## Option 2: Run EF Command on VPS

If you have `dotnet-ef` tools installed on your VPS:

### Step 1: Install EF tools (if not installed)

```bash
dotnet tool install --global dotnet-ef
```

### Step 2: Navigate to project and run migration

```bash
cd /path/to/links-backend/LinksService
dotnet ef database update
```

---

## Option 3: Auto-migrate on Application Startup

Add automatic migration to your application startup.

### Step 1: Modify `Program.cs`

Add this code after `var app = builder.Build();`:

```csharp
// Auto-apply pending migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LinksContext>();
    db.Database.Migrate();
}
```

### Step 2: Deploy and restart

Just deploy your updated code to VPS and restart the application. Migrations will be applied automatically on startup.

**Note:** This approach is convenient but less controlled. For production, Option 1 is recommended.

---

## Verify Migration Success

After running the migration, verify it worked:

### Check Categories table exists

```sql
SHOW TABLES LIKE 'Categories';
```

### Check Categories were migrated

```sql
SELECT * FROM Categories;
```

You should see all unique category names from your existing links.

### Check Links have CategoryId

```sql
SELECT Id, Title, CategoryId FROM Links LIMIT 10;
```

### Check foreign key relationship

```sql
SELECT l.Title, c.Name as CategoryName
FROM Links l
LEFT JOIN Categories c ON l.CategoryId = c.Id
LIMIT 10;
```

---

## Rollback (If Needed)

If something goes wrong, you can rollback to the previous migration:

```bash
dotnet ef database update PreviousMigrationName
```

Or generate a rollback script:

```bash
dotnet ef migrations script AddCategoryTable PreviousMigrationName -o rollback.sql
```

---

## Troubleshooting

### Error: "Unable to connect to MySQL"

Check your connection string in `appsettings.json` or environment variables.

### Error: "Table 'Categories' already exists"

The migration was partially applied. You may need to manually drop the table and re-run:

```sql
DROP TABLE IF EXISTS Categories;
```

### Error: "Column 'Category' doesn't exist"

The migration already completed. Check the `__EFMigrationsHistory` table:

```sql
SELECT * FROM __EFMigrationsHistory;
```

---

## Connection String Reference

Your connection string should look like:

```
Server=localhost;Database=links;User=your_user;Password=your_password;
```

For environment variable:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=links;User=your_user;Password=your_password;"
```
