TinTot
TinTot là một nền tảng đăng tin rao vặt được xây dựng bằng ASP.NET Core, sử dụng PostgreSQL làm cơ sở dữ liệu và hỗ trợ triển khai bằng Docker.

Tech Stack:
  ASP.NET Core Web API
  Entity Framework Core
  PostgreSQL
  Docker & Docker Compose
  Redis (tuỳ chọn nếu đã cấu hình)

Project Structure
TinTot.sln
│
├── TinTot.API                # Web API layer
├── TinTot.Application        # Business logic
├── TinTot.Domain             # Domain models
├── TinTot.Infrastructure     # Database & external services
└── docker-compose.yml

Yêu cầu hệ thống:
  .NET SDK 8.0 (hoặc version bạn đang dùng)
  Docker Desktop
  PostgreSQL (nếu không dùng Docker)
  pgAdmin (tuỳ chọn)
  
Cách chạy project

Cách 1 — Chạy bằng Docker (Khuyến nghị cho deploy)
1. Chạy toàn bộ hệ thống: docker-compose up --build
API sẽ chạy tại: http://localhost:5000
PostgreSQL:
  Host: localhost
  Port: 5432
  User: postgres
  Password: 123456
  Database: tintotdb

Cách 2 — Chạy môi trường phát triển (Debug dễ dàng)
Khuyến nghị khi develop:
  Docker chỉ chạy PostgreSQL
  API chạy bằng Visual Studio hoặc dotnet watch

1. Chạy database: docker-compose up db
2. Chạy API: dotnet watch run
(Hoặc F5 trong Visual Studio).

Cấu hình Connection String(appsettings.json)
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=tintotdb;Username=postgres;Password=123456"
}

Nếu chạy API trong Docker: Host=db

Migration
Tạo migration: dotnet ef migrations add InitialCreate --project TinTot.Infrastructure --startup-project TinTot.API
Update database: dotnet ef database update --startup-project TinTot.API

Debug
Trong môi trường phát triển:
  Chạy DB bằng Docker
  Chạy API bằng Visual Studio
  Sử dụng breakpoint bình thường
Không khuyến nghị debug trực tiếp trong container ở giai đoạn đầu.

Biến môi trường Docker(docker-compose.yml):
POSTGRES_DB=tintotdb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=123456

Tài khoản mặc định
LoginName	|  Email	                   |  Password
duyot	    | nguyennhutduy.cv@gmail.com | 123456789

Roadmap:
  Authentication (JWT)
  Role-based authorization
  CRUD bài đăng
  Upload hình ảnh
  Redis caching
  Deploy production

License
MIT License
