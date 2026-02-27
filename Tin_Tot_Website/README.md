Tin Tot Website – Backend API

Backend service cho hệ thống đăng tin (marketplace / classified platform), xây dựng bằng ASP.NET Core và PostgreSQL.
1. Tech Stack
.NET 8
ASP.NET Core Web API
Entity Framework Core
PostgreSQL
Docker (optional)
Clean Architecture (Controller → Service → Repository pattern nếu áp dụng)
Fluent API mapping (production-ready)

2. Project Structure
Tin_Tot_Website/
│
├── Controllers/
├── Models/
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/
├── Migrations/
├── Services/
├── Repositories/
├── Program.cs
└── appsettings.json

3. Database
Database: PostgreSQL
Connection string cấu hình trong:
appsettings.json

4. Setup Local Development
  1. Clone repository
     git clone https://github.com/yourname/tin-tot-core.git
     cd tin-tot-core
  2. Restore packages
     dotnet restore
  3. Run migrations
     dotnet ef database update
  4. Run project
     dotnet run

API mặc định chạy tại: https://localhost:5001
5. Entity Overview
Core entities:
  User
  Listing
  Category
  Image
  Favorite
  Follow
  Message
  Notification
  Rating
  Banner
Relationships được cấu hình bằng Fluent API trong AppDbContext.

6. Migration Commands
Tạo migration mới: dotnet ef migrations add MigrationName
Update database: dotnet ef database update
Remove migration: dotnet ef migrations remove

7. Production Notes
Không commit appsettings.Development.json chứa credentials
Sử dụng environment variables cho production secrets
Nên bật: Logging, Exception middleware, Health checks
Khuyến nghị deploy bằng Docker container

8. Docker (Optional)
Build container: docker build -t tin-tot-api .
Run container: docker run -p 8080:80 tin-tot-api

9. Future Improvements
JWT Authentication
Role-based authorization
Redis caching
Background jobs (Hangfire)
ElasticSearch for search optimization
CI/CD pipeline (GitHub Actions)

10. License
Private project.
