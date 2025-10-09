Brightness Backend
Yêu cầu

.NET 8 SDK

PostgreSQL 14+

(Tùy chọn) Docker & Docker Compose nếu muốn chạy Postgres qua container

Cấu trúc chính

AuthenticationAPI/ – microservice đăng nhập, đăng ký người dùng

GatewayAPI/ – API Gateway entrypoint

BrightNess.SharedLibrarySolution/ – thư viện dùng chung (JWT, DI, Serilog, …)

Thiết lập môi trường

Clone repo:

git clone https://github.com/<your-org>/Brightness_be.git
cd Brightness_be

Khôi phục package:

dotnet restore

Cấu hình Postgres:

Tạo database BrightnessDb.

Cập nhật chuỗi kết nối trong AuthenticationAPI/AuthenticationAPI.Presentation/appsettings.json và appsettings.Development.json nếu user/pass khác mặc định (Username=postgres;Password=postgres).
S? d?ng file .env (v� d? ConnectionStrings__BrightnessDb ho?c c�c bi?n DB_HOST/DB_PORT/DB_USER/DB_PASSWORD) thay v� ch?nh s?a appsettings.* d? c?u h�nh th�ng tin nh?y c?m.

Chạy migration & cập nhật database
cd AuthenticationAPI/AuthenticationAPI.Infrastructure

# Tạo migration mới khi thay đổi model:

# dotnet ef migrations add <MigrationName> --startup-project ..\AuthenticationAPI.Presentation

# Áp dụng schema hiện tại vào BrightnessDb:

dotnet ef database update --startup-project ..\AuthenticationAPI.Presentation

Lưu ý: Mỗi migration nằm trong AuthenticationAPI.Infrastructure/Migrations. Startup project phải là AuthenticationAPI.Presentation để nạp DI & connection string.

Chạy dịch vụ Authentication
cd AuthenticationAPI/AuthenticationAPI.Presentation
dotnet run

Swagger sẽ lắng nghe tại https://localhost:5001/swagger (hoặc cổng bạn cấu hình).

Kiểm thử nhanh

POST /api/authentication/register với payload JSON hợp lệ (email, userName, password, role, status, …)

POST /api/authentication/login để lấy JWT token

GET /api/authentication/{id} (kèm Bearer token) để lấy thông tin user

Ghi chú

Các thành phần khác (Gateway, Payment, …) cần cấu hình chuỗi kết nối riêng và chạy migrations tương ứng.

Khi chuyển môi trường (staging/production), đặt secret qua user-secrets hoặc biến môi trường thay vì commit thẳng vào appsettings.
