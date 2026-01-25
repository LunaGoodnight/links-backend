# Links Management System

A full-stack CRUD system for managing links with optional images, consisting of three separate projects:

- **links-backend**: .NET 9 API with MySQL
- **links-frontend**: Next.js 16 public website
- **links-admin**: Next.js 16 admin panel

## Architecture Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  links-frontend │     │   links-admin   │     │   DO Spaces     │
│   (Next.js 16)  │     │   (Next.js 16)  │     │   (S3 Images)   │
│    Port 3004    │     │    Port 4002    │     │                 │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         │    ┌──────────────────┴───────────────────────┘
         │    │
         ▼    ▼
┌─────────────────┐
│  links-backend  │
│   (.NET 9 API)  │
│    Port 5005    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│     MySQL 8.4   │
│    (Database)   │
└─────────────────┘
```

## Data Model

### Link Entity
| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Title | string | Required, max 200 chars |
| Url | string | Required, max 2000 chars |
| Description | string | Optional, max 1000 chars |
| Category | string | Optional, max 100 chars |
| Tags | List\<string\> | Stored as comma-separated |
| ImageUrl | string | Optional, DO Spaces URL |
| CreatedAt | DateTime | UTC timestamp |
| UpdatedAt | DateTime | UTC timestamp |
| Order | int | For custom sorting |

### AdminUser Entity
| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key |
| Username | string | Unique |
| PasswordHash | string | BCrypt hashed |
| CreatedAt | DateTime | UTC timestamp |

## Project 1: links-backend

### Tech Stack
- .NET 9 Web API
- Entity Framework Core 9 with MySQL (Pomelo)
- JWT Authentication
- AWS S3 SDK (for DO Spaces)
- BCrypt for password hashing

### API Endpoints

#### Public Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/links` | List all links (with optional filters) |
| GET | `/api/links/{id}` | Get single link |
| GET | `/api/categories` | List all categories |
| GET | `/api/categories/tags` | List all tags |

#### Query Parameters for `/api/links`
- `category` - Filter by category
- `tag` - Filter by tag
- `search` - Search in title and description

#### Admin Endpoints (JWT Required)
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Admin login, returns JWT |
| POST | `/api/links` | Create link (JSON body) |
| POST | `/api/links/upload` | Create link with image (multipart) |
| PUT | `/api/links/{id}` | Update link (JSON body) |
| PUT | `/api/links/{id}/upload` | Update link with image (multipart) |
| DELETE | `/api/links/{id}` | Delete link (also removes image) |

### Setup

1. Copy environment file:
```bash
cp .env.example .env
```

2. Configure `.env`:
```env
MYSQL_ROOT_PASSWORD=your_root_password
MYSQL_DATABASE=links
MYSQL_USER=links
MYSQL_PASSWORD=your_db_password

JWT_SECRET=your-super-secret-jwt-key-at-least-32-characters-long

AWS_ACCESS_KEY=your_do_spaces_access_key
AWS_SECRET_KEY=your_do_spaces_secret_key
AWS_SERVICE_URL=https://sgp1.digitaloceanspaces.com
AWS_BUCKET_NAME=your_bucket_name

ADMIN_USERNAME=admin
ADMIN_PASSWORD=your_secure_password
```

3. Run with Docker:
```bash
docker compose up -d
```

4. Or run locally:
```bash
cd LinksService
dotnet run
```

### Local Development
```bash
# Build
dotnet build LinksService/LinksService.csproj

# Run
dotnet run --project LinksService/LinksService.csproj

# Create migration
dotnet ef migrations add MigrationName --project LinksService

# Apply migrations
dotnet ef database update --project LinksService
```

---

## Project 2: links-frontend

### Tech Stack
- Next.js 16 (App Router)
- React 19
- TypeScript
- Tailwind CSS 4

### Features
- Server-side rendering for SEO
- Responsive grid layout
- Category filtering
- Search functionality
- Image display with Next.js Image optimization

### Setup

1. Install dependencies:
```bash
cd ../links-frontend
pnpm install
```

2. Create `.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5005
```

3. Run development server:
```bash
pnpm dev
```

4. Build for production:
```bash
pnpm build
```

5. Run with Docker:
```bash
docker compose up -d
```

### Routes
| Route | Description |
|-------|-------------|
| `/` | Home page with all links |
| `/category/[slug]` | Links filtered by category |

---

## Project 3: links-admin

### Tech Stack
- Next.js 16 (App Router)
- React 19
- TypeScript
- Tailwind CSS 4

### Features
- JWT authentication with httpOnly cookies
- Middleware route protection
- Dashboard with statistics
- CRUD table for links
- Image upload with preview
- Tag management

### Setup

1. Install dependencies:
```bash
cd ../links-admin
pnpm install
```

2. Create `.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5005
```

3. Run development server:
```bash
pnpm dev
```

4. Build for production:
```bash
pnpm build
```

5. Run with Docker:
```bash
docker compose up -d
```

### Routes
| Route | Description |
|-------|-------------|
| `/login` | Admin login page |
| `/dashboard` | Dashboard with stats |
| `/links` | Links list with CRUD |
| `/links/new` | Create new link |
| `/links/[id]/edit` | Edit existing link |

---

## Deployment

### Prerequisites

- Docker and Docker Compose installed
- A server (VPS) for production (e.g., DigitalOcean, AWS EC2, Linode)
- Domain name (optional, for production)
- DigitalOcean Spaces or S3-compatible storage for images

### Quick Start with Docker Compose

**1. Clone and configure:**
```bash
git clone <your-repo-url>
cd links-backend
cp .env.example .env
```

**2. Edit `.env` with your values:**
```env
# MySQL Configuration
MYSQL_ROOT_PASSWORD=strong_root_password_here
MYSQL_DATABASE=links
MYSQL_USER=links
MYSQL_PASSWORD=strong_db_password_here

# JWT Configuration (minimum 32 characters)
JWT_SECRET=your-super-secret-jwt-key-at-least-32-characters-long

# AWS/DigitalOcean Spaces Configuration
AWS_ACCESS_KEY=your_access_key
AWS_SECRET_KEY=your_secret_key
AWS_SERVICE_URL=https://sgp1.digitaloceanspaces.com
AWS_BUCKET_NAME=your_bucket

# Admin User (created on first startup)
ADMIN_USERNAME=admin
ADMIN_PASSWORD=your_secure_admin_password
```

**3. Start the services:**
```bash
docker compose up -d
```

**4. Verify deployment:**
```bash
# Check container status
docker compose ps

# View logs
docker compose logs -f

# Test API
curl http://localhost:5005/api/links
```

### Environment Variables Reference

| Variable | Required | Description |
|----------|----------|-------------|
| `MYSQL_ROOT_PASSWORD` | Yes | MySQL root password |
| `MYSQL_DATABASE` | Yes | Database name |
| `MYSQL_USER` | Yes | Database user |
| `MYSQL_PASSWORD` | Yes | Database password |
| `JWT_SECRET` | Yes | JWT signing key (32+ chars) |
| `AWS_ACCESS_KEY` | Yes | S3/Spaces access key |
| `AWS_SECRET_KEY` | Yes | S3/Spaces secret key |
| `AWS_SERVICE_URL` | Yes | S3 endpoint URL |
| `AWS_BUCKET_NAME` | Yes | Storage bucket name |
| `ADMIN_USERNAME` | Yes | Initial admin username |
| `ADMIN_PASSWORD` | Yes | Initial admin password |

### Docker Commands

```bash
# Start services
docker compose up -d

# Stop services
docker compose down

# Rebuild after code changes
docker compose up -d --build

# View logs
docker compose logs -f

# View specific service logs
docker compose logs -f api

# Restart a service
docker compose restart api

# Remove volumes (WARNING: deletes data)
docker compose down -v
```

### Production Deployment

#### Option 1: Single Server

1. SSH into your server
2. Install Docker and Docker Compose
3. Clone the repository
4. Configure `.env` file
5. Run `docker compose up -d`
6. Set up a reverse proxy (Nginx/Caddy) for HTTPS

#### Option 2: Separate Services

Deploy each component separately:

```bash
# Backend (includes MySQL)
cd links-backend
docker compose up -d
# API available at http://localhost:5005

# Frontend
cd links-frontend
docker compose up -d
# Site available at http://localhost:3004

# Admin
cd links-admin
docker compose up -d
# Admin panel available at http://localhost:4002
```

### Domain & SSL Configuration

**When to configure:** After Docker containers are running and verified working on localhost.

#### Step 1: Point DNS to Your Server

Add DNS A records pointing to your server IP:
```
links-api.yourdomain.com    → YOUR_SERVER_IP
links.yourdomain.com        → YOUR_SERVER_IP
links-admin.yourdomain.com  → YOUR_SERVER_IP
```

#### Step 2: Install Nginx & Certbot

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install nginx certbot python3-certbot-nginx

# Start Nginx
sudo systemctl start nginx
sudo systemctl enable nginx
```

#### Step 3: Configure Nginx (without SSL first)

Create `/etc/nginx/sites-available/links-api`:
```nginx
server {
    listen 80;
    server_name links-api.yourdomain.com;

    location / {
        proxy_pass http://127.0.0.1:5005;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Enable the site:
```bash
sudo ln -s /etc/nginx/sites-available/links-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### Step 4: Obtain SSL Certificate

```bash
# Get SSL certificate (auto-configures Nginx)
sudo certbot --nginx -d links-api.yourdomain.com

# Verify auto-renewal
sudo certbot renew --dry-run
```

#### Step 5: Update Frontend Environment

After SSL is configured, update your frontend `.env.local`:
```env
NEXT_PUBLIC_API_URL=https://links-api.yourdomain.com
```

### Production with Nginx Reverse Proxy (Full Example)

Example nginx configuration with SSL (Certbot will add SSL directives automatically):
```nginx
# API - /etc/nginx/sites-available/links-api
server {
    listen 80;
    server_name links-api.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name links-api.yourdomain.com;

    # SSL certificates (managed by Certbot)
    ssl_certificate /etc/letsencrypt/live/links-api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/links-api.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5005;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Frontend - /etc/nginx/sites-available/links-frontend
server {
    listen 80;
    server_name links.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name links.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/links.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/links.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:3004;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# Admin - /etc/nginx/sites-available/links-admin
server {
    listen 80;
    server_name links-admin.yourdomain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name links-admin.yourdomain.com;

    ssl_certificate /etc/letsencrypt/live/links-admin.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/links-admin.yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:4002;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## DigitalOcean Spaces Setup

1. Create a Space in your DO account
2. Generate Spaces access keys
3. Configure CORS for your domains:
```json
{
  "CORSRules": [
    {
      "AllowedOrigins": ["https://links.yourdomain.com", "https://links-admin.yourdomain.com"],
      "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
      "AllowedHeaders": ["*"]
    }
  ]
}
```
4. Enable CDN for faster image delivery

---

## Troubleshooting

### Common Issues

**Container won't start:**
```bash
# Check logs for errors
docker compose logs api

# Verify environment variables
docker compose config
```

**Database connection failed:**
```bash
# Wait for MySQL to be ready (healthcheck)
docker compose logs db

# Verify MySQL is healthy
docker compose ps
```

**Permission denied on volumes:**
```bash
# Fix volume permissions (Linux)
sudo chown -R 1000:1000 ./data
```

**Port already in use:**
```bash
# Find process using port 5005
netstat -tlnp | grep 5005

# Or change port in compose.yaml
ports:
  - "127.0.0.1:5006:8080"
```

**Image upload fails:**
- Verify AWS/Spaces credentials in `.env`
- Check bucket permissions and CORS settings
- Ensure CDN URL is correct

### Health Check

```bash
# API health
curl http://localhost:5005/api/links

# Database connection (from inside container)
docker compose exec api dotnet ef database update --dry-run
```

### Backup & Restore

```bash
# Backup database
docker compose exec db mysqldump -u root -p links > backup.sql

# Restore database
docker compose exec -T db mysql -u root -p links < backup.sql
```

---

## Quick Start (Development)

```bash
# Terminal 1: Start backend
cd links-backend
docker compose up

# Terminal 2: Start frontend
cd links-frontend
echo "NEXT_PUBLIC_API_URL=http://localhost:5005" > .env.local
pnpm dev

# Terminal 3: Start admin
cd links-admin
echo "NEXT_PUBLIC_API_URL=http://localhost:5005" > .env.local
pnpm dev
```

Default admin credentials (change in production!):
- Username: `admin`
- Password: `admin123`

---

## Directory Structure

```
links-backend/
├── LinksService/
│   ├── Controllers/
│   │   ├── LinksController.cs
│   │   ├── CategoriesController.cs
│   │   └── AuthController.cs
│   ├── Services/
│   │   ├── IImageUploadService.cs
│   │   └── ImageUploadService.cs
│   ├── Data/
│   │   └── LinksContext.cs
│   ├── Models/
│   │   ├── Link.cs
│   │   ├── AdminUser.cs
│   │   └── DTOs/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
├── Dockerfile
├── compose.yaml
└── .env.example

links-frontend/
├── src/
│   ├── app/
│   │   ├── page.tsx
│   │   ├── layout.tsx
│   │   └── category/[slug]/page.tsx
│   ├── components/
│   │   ├── LinkCard.tsx
│   │   ├── LinkGrid.tsx
│   │   ├── CategoryFilter.tsx
│   │   └── SearchBar.tsx
│   ├── lib/
│   │   └── api.ts
│   └── types/
│       └── link.ts
├── Dockerfile
├── compose.yaml
└── next.config.ts

links-admin/
├── src/
│   ├── app/
│   │   ├── page.tsx
│   │   ├── layout.tsx
│   │   ├── login/
│   │   ├── dashboard/
│   │   └── links/
│   ├── components/
│   │   ├── Sidebar.tsx
│   │   ├── LinkForm.tsx
│   │   ├── LinkTable.tsx
│   │   └── ImageUpload.tsx
│   ├── lib/
│   │   ├── api.ts
│   │   └── auth.ts
│   ├── types/
│   │   └── link.ts
│   └── middleware.ts
├── Dockerfile
├── compose.yaml
└── next.config.ts
```

---

## License

MIT
