# Category CRUD Implementation

This document describes the implementation of full CRUD functionality for Categories across the `links-backend` (ASP.NET Core) and `links-admin`/`links-frontend` (Next.js) projects.

## Overview

Previously, categories were stored as simple strings on the Link model. This implementation creates a proper Category entity with full CRUD operations and establishes a foreign key relationship between Links and Categories.

---

## Backend Changes (links-backend)

### New Files

#### 1. `Models/Category.cs`
Category entity with the following properties:
- `Id` (int) - Primary key
- `Name` (string, required, max 100 chars) - Category name with unique constraint
- `Description` (string?, max 500 chars) - Optional description
- `Order` (int) - Sort order
- `CreatedAt` (DateTime) - Creation timestamp
- `UpdatedAt` (DateTime) - Last update timestamp

#### 2. `Models/DTOs/CategoryDto.cs`
Three DTOs for category operations:
- `CategoryDto` - Response DTO with all fields
- `CreateCategoryDto` - For creating categories (name required)
- `UpdateCategoryDto` - For updating categories (all fields optional)

#### 3. `Migrations/20260202153439_AddCategoryTable.cs`
Database migration that:
1. Creates the Categories table
2. Adds CategoryId column to Links table
3. Migrates existing category strings to Category records
4. Updates Links to reference new CategoryId values
5. Removes old Category string column
6. Creates indexes and foreign key constraint

### Modified Files

#### 1. `Data/LinksContext.cs`
- Added `DbSet<Category> Categories`
- Added unique constraint on `Category.Name`
- Added index on `Category.Order`
- Configured Link-Category FK relationship with `OnDelete: SetNull`

#### 2. `Models/Link.cs`
Changed from:
```csharp
public string? Category { get; set; }
```
To:
```csharp
public int? CategoryId { get; set; }
public Category? Category { get; set; }  // Navigation property
```

#### 3. `Models/DTOs/LinkDto.cs`
- `LinkDto`: Changed `Category` to `CategoryId` and `CategoryName`
- `CreateLinkDto`: Changed `Category` to `CategoryId`
- `UpdateLinkDto`: Changed `Category` to `CategoryId`

#### 4. `Controllers/CategoriesController.cs`
Replaced simple GET endpoint with full CRUD:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/categories` | No | List all categories (ordered by Order, then Name) |
| GET | `/api/categories/{id}` | No | Get single category by ID |
| POST | `/api/categories` | Yes | Create new category |
| PUT | `/api/categories/{id}` | Yes | Update existing category |
| DELETE | `/api/categories/{id}` | Yes | Delete category (sets Links.CategoryId to null) |
| GET | `/api/categories/tags` | No | List all unique tags (unchanged) |

#### 5. `Controllers/LinksController.cs`
- Changed query parameter from `category` (string) to `categoryId` (int)
- Added `.Include(l => l.Category)` for eager loading
- Updated all CRUD operations to use `CategoryId`
- Response includes both `CategoryId` and `CategoryName`

---

## Frontend Changes (links-admin)

### New Files

#### 1. `types/category.ts`
TypeScript interfaces:
```typescript
interface Category {
  id: number;
  name: string;
  description?: string;
  order: number;
  createdAt: string;
  updatedAt: string;
}

interface CreateCategoryInput { ... }
interface UpdateCategoryInput { ... }
```

#### 2. `components/CategoryTable.tsx`
Table component displaying categories with:
- Name (as badge)
- Description
- Order
- Edit/Delete action buttons

#### 3. `components/CategoryForm.tsx`
Form component with fields:
- Name (required)
- Description (textarea)
- Order (number)

#### 4. `app/categories/layout.tsx`
Layout wrapper with Sidebar navigation.

#### 5. `app/categories/page.tsx`
Category list page with "Add Category" button.

#### 6. `app/categories/new/page.tsx`
Create new category page with CategoryForm.

#### 7. `app/categories/[id]/edit/page.tsx`
Edit category page with CategoryForm pre-populated.

### Modified Files

#### 1. `types/link.ts`
Changed Link interface:
```typescript
// Before
category?: string;

// After
categoryId?: number;
categoryName?: string;
```

#### 2. `lib/api.ts`
- Added `getCategory(id)` function
- Added `createCategory(data, token)` function
- Added `updateCategory(id, data, token)` function
- Added `deleteCategory(id, token)` function
- Updated `getCategories()` return type to `Category[]`

#### 3. `components/Sidebar.tsx`
- Added "Categories" navigation item with tag icon

#### 4. `components/LinkForm.tsx`
- Changed category input from text field with datalist to `<select>` dropdown
- Uses `categoryId` instead of category string

#### 5. `components/LinkTable.tsx`
- Changed to display `link.categoryName` instead of `link.category`

#### 6. `app/links/new/page.tsx` & `app/links/[id]/edit/page.tsx`
- Updated form data handling to use `categoryId`

---

## Frontend Changes (links-frontend)

### Modified Files

#### 1. `types/link.ts`
- Added `Category` interface
- Changed Link to use `categoryId` and `categoryName`

#### 2. `lib/api.ts`
- Updated `getLinks()` parameter from `category` (string) to `categoryId` (number)
- Updated `getCategories()` return type to `Category[]`

#### 3. `lib/mockData.ts`
- Changed `FEATURED_CATEGORIES` from string array to Category array
- Updated `generateMockLinks()` to use Category objects

#### 4. `components/CategoryFilter.tsx`
- Changed props from `categories: string[]` to `categories: Category[]`
- Changed `currentCategory` to `currentCategoryId`
- Uses `categoryId` query parameter instead of `category`

#### 5. `components/LinkCard.tsx`
- Changed to display `link.categoryName`

#### 6. `app/page.tsx`
- Updated to use `category.id` for API calls
- Updated to display `category.name`

#### 7. `app/links/page.tsx`
- Changed query parameter from `category` to `categoryId`
- Updated CategoryFilter props

### Deleted Files

#### `app/links/category/[slug]/`
Removed the category slug routing. Category filtering now uses query parameters (`?categoryId=1`) on the main `/links` page.

---

## Database Migration

To apply the migration, run:
```bash
cd links-backend/LinksService
dotnet ef database update
```

The migration automatically:
1. Creates the Categories table
2. Extracts unique category names from existing Links
3. Creates Category records for each unique name
4. Updates Links.CategoryId to reference the new Category records
5. Removes the old Category string column

---

## API Usage Examples

### Categories API

```bash
# Get all categories
curl http://localhost:5005/api/categories

# Get single category
curl http://localhost:5005/api/categories/1

# Create category (requires auth)
curl -X POST http://localhost:5005/api/categories \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"name": "Tools", "description": "Useful tools", "order": 0}'

# Update category (requires auth)
curl -X PUT http://localhost:5005/api/categories/1 \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"name": "Dev Tools", "order": 1}'

# Delete category (requires auth)
curl -X DELETE http://localhost:5005/api/categories/1 \
  -H "Authorization: Bearer <token>"
```

### Links API (Updated)

```bash
# Get links filtered by category ID
curl http://localhost:5005/api/links?categoryId=1

# Create link with category
curl -X POST http://localhost:5005/api/links \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{"title": "Example", "url": "https://example.com", "categoryId": 1}'
```

---

## Testing Checklist

### Backend
- [ ] GET /api/categories returns all categories
- [ ] GET /api/categories/{id} returns single category
- [ ] POST /api/categories creates new category
- [ ] POST /api/categories with duplicate name returns 409 Conflict
- [ ] PUT /api/categories/{id} updates category
- [ ] DELETE /api/categories/{id} deletes category and nullifies Links.CategoryId
- [ ] GET /api/links?categoryId=X filters by category
- [ ] Link responses include categoryId and categoryName

### Frontend (links-admin)
- [ ] Categories page lists all categories
- [ ] Create new category works
- [ ] Edit category works
- [ ] Delete category works with confirmation
- [ ] Link form shows category dropdown
- [ ] Creating/editing links with category works

### Frontend (links-frontend)
- [ ] Home page displays categories correctly
- [ ] Links page category filter works
- [ ] Link cards display category name
