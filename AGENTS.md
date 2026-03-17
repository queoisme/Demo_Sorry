# AGENTS.md - AppSorry Backend Development Guide

## Project Overview
- **Project Type**: ASP.NET Core 9.0 Web API
- **Framework**: .NET 9.0
- **Database**: Supabase (PostgreSQL) với Entity Framework Core
- **Primary Features**: 
  - Upload hình ảnh từ máy tính với caption và emoji
  - Upload nhạc từ máy tính hoặc lấy từ YouTube
  - Phát nhạc nền khi truy cập trang web

## Project Structure
```
AppSorry/
├── Controllers/       # API endpoints
├── Models/           # Entity classes
├── DTOs/             # Data Transfer Objects
├── Services/         # Business logic
├── Data/             # DbContext
└── wwwroot/          # Static files (uploads)
```

## Build, Run & Test Commands

### Build Project
```bash
cd AppSorry
dotnet build
```

### Run Development Server
```bash
dotnet run
# Hoặc với hot reload
dotnet watch run
```
API available at: `http://localhost:5000`
Swagger UI: `http://localhost:5000/swagger`

### Run Single Test
```bash
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
```

### Run All Tests
```bash
dotnet test
```

### Clean & Rebuild
```bash
dotnet clean
dotnet build
```

## Package Dependencies
- `Npgsql.EntityFrameworkCore.PostgreSQL` - Database (Supabase/PostgreSQL)
- `Microsoft.EntityFrameworkCore.Design` - EF migrations
- `YoutubeExplode` v6.3.2 - YouTube audio extraction
- `YoutubeExplode.Converter` v6.3.2 - YouTube audio conversion
- `Xabe.FFmpeg` v5.2.6 - Media processing

## Database Connection
- Connection string được cấu hình trong `appsettings.json`
- Format: `Host=...;Port=5432;Database=postgres;Username=postgres;Password=...`
- Supabase URL: `db.jgbzilnpalinscqwjuuk.supabase.co`

## Code Style Guidelines

### Naming Conventions
- **Classes/Models**: PascalCase (`MediaItem`, `BackgroundMusic`)
- **Interfaces**: PascalCase với prefix `I` (`IMediaService`)
- **Methods**: PascalCase (`GetAllMedia`, `UploadImage`)
- **DTOs**: PascalCase với suffix `Dto` (`CreateMediaItemDto`)
- **Variables**: camelCase (`mediaItems`, `filePath`)
- **Private fields**: camelCase với prefix `_` (`_context`, `_storagePath`)
- **Constants**: PascalCase (`MaxFileSize`, `AllowedImageExtensions`)

### C# Conventions
- Use **file-scoped namespaces**: `namespace AppSorry.Models;`
- Use **implicit usings** (enabled in csproj)
- Use **nullable reference types**: `<Nullable>enable</Nullable>`
- Use **primary constructors** where appropriate
- Use **var** for local variable type inference
- Use **string interpolation** instead of string concatenation

### Imports/Using Statements
```csharp
using AppSorry.Models;
using AppSorry.DTOs;
using AppSorry.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
```

### Formatting
- Use **4 spaces** for indentation (default in Visual Studio/.editorconfig)
- Add trailing commas in multiline lists
- Group related using statements
- One namespace per file
- Class braces on new line

### Types
- Always specify return types for methods
- Use `Task<T>` for async methods
- Use `IActionResult` for controller actions
- Use `string?` for nullable strings
- Use proper primitive types: `int`, `bool`, `DateTime`

### Error Handling
- Use `try-catch` with proper logging in Services
- Return appropriate HTTP status codes:
  - `200 OK` - Success
  - `201 Created` - Resource created
  - `400 BadRequest` - Invalid input
  - `404 NotFound` - Resource not found
  - `500 InternalServerError` - Server error
- Use `ProblemDetails` for error responses
- Always validate input in Controllers before calling Services

### API Controller Guidelines
- Use `[ApiController]` attribute
- Use route attributes: `[Route("api/[controller]")]`
- Use HTTP method attributes: `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
- Return `ActionResult<T>` for type-safe responses
- Use `[FromForm]` for multipart form data
- Document with XML comments for Swagger

### Entity Framework Core
- Use DbContext for database operations
- Create migrations: `dotnet ef migrations add <Name>`
- Apply migrations: `dotnet ef database update`
- Use async/await for all database operations
- Include related data: `_context.Items.Include(x => x.Related).ToListAsync()`

### File Handling
- Validate file extensions and MIME types
- Generate unique filenames (GUID + extension)
- Store files in `wwwroot/uploads/` directory
- Return file URLs in API responses
- Implement file size limits (max 10MB for images, 50MB for audio)

### YouTube Integration
- Validate YouTube URL format
- Extract video/audio using YoutubeExplode
- Download audio in background task
- Store downloaded files locally for caching

## Development Workflow

### Adding New Feature
1. Create Model in `Models/`
2. Create DTO in `DTOs/`
3. Add DbSet to `Data/AppDbContext`
4. Create Service interface in `Services/`
5. Implement Service
6. Create Controller endpoint
7. Add migration: `dotnet ef migrations add <FeatureName>`
8. Test with Swagger

### Database Migrations
```bash
# Add migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# List migrations
dotnet ef migrations list
```

## Common Tasks

### Upload Image
- Endpoint: `POST /api/media/upload-image`
- Content-Type: `multipart/form-data`
- Fields: `file` (IFormFile), `caption` (string), `emojis` (string)

### Upload Audio File
- Endpoint: `POST /api/music/upload-audio`
- Content-Type: `multipart/form-data`
- Fields: `file` (IFormFile), `title` (string)

### Add YouTube Music
- Endpoint: `POST /api/music/youtube`
- Body: `{ "youtubeUrl": "string" }`

### Get All Media
- Endpoint: `GET /api/media`

### Get Background Music
- Endpoint: `GET /api/music`

## Testing
- Use xUnit as test framework
- Place tests in `AppSorry.Tests/` project
- Follow naming: `<ClassName>Tests`
- Use `[Fact]` and `[Theory]` attributes
- Mock dependencies with Moq

## API Documentation
- Swagger UI enabled at `/swagger`
- Use XML comments for endpoint descriptions
- Document response codes with `[ProducesResponseType]`

## Deployment

### Deploy lên Railway (qua GitHub)

#### Bước 1: Push code lên GitHub
```bash
# Tạo GitHub repo mới và push
git init
git add .
git commit -m "Initial commit"
gh repo create appsorry-backend --public --source=. --push
```

#### Bước 2: Connect GitHub với Railway
1. Vào https://railway.com
2. Tạo project mới → "Deploy from GitHub repo"
3. Chọn repo `appsorry-backend`

#### Bước 3: Cấu hình Environment Variables
Trong Railway Dashboard → Variables, thêm:
```
ConnectionStrings__DefaultConnection=Host=db.jgbzilnpalinscqwjuuk.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Thien13879428
ASPNETCORE_ENVIRONMENT=Production
```

#### Bước 4: Deploy
- Railway sẽ tự động build từ Dockerfile
- Sau khi deploy xong, API available tại URL Railway cung cấp
- Swagger: `<railway-url>/swagger`

#### Files cần thiết cho Railway:
- `AppSorry/Dockerfile` - Multi-stage build cho .NET 9.0
- `railway.json` - Cấu hình Railway project

#### Production Notes
- Database: Supabase (đã có, không cần deploy riêng)
- Static files: Lưu trong container, có thể mất khi restart
- Nên dùng Supabase Storage cho static files nếu cần persistence
