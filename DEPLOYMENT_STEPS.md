# Deployment Steps for Category CRUD

## Prerequisites

- VPS with MySQL running
- SSH access to VPS
- Backend project deployed on VPS

---

## Step 1: Copy Migration Script to VPS

From your local machine:

```bash
scp D:/side-project/links-backend/LinksService/migration.sql user@your-vps:/tmp/
```

Or using Windows PowerShell:

```powershell
scp D:\side-project\links-backend\LinksService\migration.sql user@your-vps:/tmp/
```

---

## Step 2: Run Migration on VPS

SSH into your VPS:

```bash
ssh user@your-vps
```

Run the migration:

```bash
mysql -u your_mysql_user -p your_database_name < /tmp/migration.sql
```

You will be prompted for the MySQL password.

---

## Step 3: Verify Migration

Check that the Categories table was created:

```bash
mysql -u your_mysql_user -p your_database_name -e "SELECT * FROM Categories;"
```

Check that Links have CategoryId:

```bash
mysql -u your_mysql_user -p your_database_name -e "SELECT Id, Title, CategoryId FROM Links LIMIT 5;"
```

---

## Step 4: Deploy Updated Code

Deploy the updated backend and frontend code to your VPS:

### Backend (links-backend)

```bash
cd /path/to/links-backend/LinksService
git pull origin main
dotnet publish -c Release -o /path/to/publish
# Restart your service (systemd, docker, etc.)
sudo systemctl restart links-backend
```

### Admin Frontend (links-admin)

```bash
cd /path/to/links-admin
git pull origin main
npm install
npm run build
# Restart if using PM2
pm2 restart links-admin
```

### Public Frontend (links-frontend)

```bash
cd /path/to/links-frontend
git pull origin main
npm install
npm run build
# Restart if using PM2
pm2 restart links-frontend
```

---

## Step 5: Test the Application

### Test Backend API

```bash
# Get all categories
curl http://localhost:5005/api/categories

# Get all links (should include categoryId and categoryName)
curl http://localhost:5005/api/links
```

### Test Admin Panel

1. Open admin panel in browser
2. Check "Categories" link appears in sidebar
3. Try creating a new category
4. Try editing a link and selecting a category from dropdown

### Test Public Frontend

1. Open public site in browser
2. Check category filter works
3. Check links display category names

---

## Rollback (If Needed)

If something goes wrong, you can restore from backup or run:

```sql
-- Remove foreign key
ALTER TABLE Links DROP FOREIGN KEY FK_Links_Categories_CategoryId;

-- Add back Category string column
ALTER TABLE Links ADD Category varchar(100) NULL;

-- Restore category names from Categories table
UPDATE Links l
INNER JOIN Categories c ON c.Id = l.CategoryId
SET l.Category = c.Name;

-- Drop CategoryId column
ALTER TABLE Links DROP COLUMN CategoryId;

-- Drop Categories table
DROP TABLE Categories;

-- Remove migration history entry
DELETE FROM __EFMigrationsHistory WHERE MigrationId = '20260202153439_AddCategoryTable';
```

---

## Summary Checklist

- [ ] Copy `migration.sql` to VPS
- [ ] Run migration on MySQL
- [ ] Verify Categories table created
- [ ] Verify existing categories migrated
- [ ] Deploy backend code
- [ ] Deploy admin frontend code
- [ ] Deploy public frontend code
- [ ] Test category CRUD in admin panel
- [ ] Test category filter on public site
