# Hướng dẫn Deploy Microservices và API Gateway lên AWS EC2

Tài liệu này hướng dẫn các bước để deploy một hệ thống microservices (.NET) và một API Gateway (Ocelot) lên cùng một máy ảo (instance) EC2 duy nhất bằng Docker.

## Section 1: Build và Push Docker Image

Quá trình này được thực hiện ở **máy tính cá nhân** của bạn. Mỗi khi có thay đổi trong code, bạn cần lặp lại các bước này cho service tương ứng.

#### 1. Build Docker Image
Mở terminal ở thư mục gốc của dự án và chạy lệnh build. Sử dụng cờ `-f` để chỉ định đường dẫn đến Dockerfile.

*Đối với API Gateway:*
```shell
docker build -t my-app/gateway-api -f GatewayAPI/APIGateway.Presentation/Dockerfile .
```

*Đối với một microservice (ví dụ: AuthenticationAPI):*
```shell
docker build -t my-app/authentication-api -f AuthenticationAPI/AuthenticationAPI.Presentation/Dockerfile .
```

#### 2. Tag Image cho Amazon ECR
Gắn tag cho image vừa build với định dạng của Amazon ECR.

*Đối với API Gateway:*
```shell
docker tag my-app/gateway-api:latest <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/gateway-api:latest
```

*Đối với AuthenticationAPI:*
```shell
docker tag my-app/authentication-api:latest <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/authentication-api:latest
```

#### 3. Push Image lên ECR
Đẩy image đã được tag lên ECR.

*Đối với API Gateway:*
```shell
docker push <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/gateway-api:latest
```

*Đối với AuthenticationAPI:*
```shell
docker push <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/authentication-api:latest
```

## Section 2: Cấu hình và Kết nối EC2

#### 1. Cấu hình Security Group
Đảm bảo Security Group của máy ảo EC2 đã mở các cổng sau:
- **Port 22 (SSH):** Source là `My IP` để bạn có thể SSH vào.
- **Port 80 (HTTP):** Source là `Anywhere - IPv4` (`0.0.0.0/0`) để người dùng có thể truy cập vào gateway.

#### 2. Kết nối vào EC2 bằng SSH
Sử dụng lệnh sau, thay thế đường dẫn đến file key và IP public của bạn.
```shell
ssh -i "path/to/your-key.pem" ec2-user@<YOUR_EC2_PUBLIC_IP>
```

## Section 3: Deploy Containers trên EC2

Các lệnh sau được thực hiện **trên máy chủ EC2** sau khi đã SSH thành công.

#### 1. Đăng nhập Docker vào ECR
Lệnh này cần được chạy lại mỗi 12 tiếng vì token sẽ hết hạn.
```shell
aws ecr get-login-password --region <YOUR_AWS_REGION> | docker login --username AWS --password-stdin <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com
```

#### 2. Tạo Mạng Docker
Tạo một mạng ảo để các container có thể giao tiếp với nhau qua tên.
```shell
docker network create my-app-net
```

#### 3. Chạy các Microservices
Chạy các container của microservice trên mạng đã tạo, **không mở cổng** ra bên ngoài.
```shell
# Dừng và xóa container cũ nếu có
docker stop authentication-api
docker rm authentication-api

# Tải về image mới nhất
docker pull <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/authentication-api:latest

# Chạy container
docker run -d --name authentication-api --network my-app-net <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/authentication-api:latest
```

#### 4. Chạy API Gateway
Chạy container gateway, **mở cổng 80** của máy chủ và map vào cổng 5000 của container.
```shell
# Dừng và xóa container cũ nếu có
docker stop gateway-api
docker rm gateway-api

# Tải về image mới nhất
docker pull <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/gateway-api:latest

# Chạy container và mở cổng
docker run -d -p 80:5000 --name gateway-api --network my-app-net <YOUR_AWS_ACCOUNT_ID>.dkr.ecr.<YOUR_AWS_REGION>.amazonaws.com/my-app/gateway-api:latest
```

## Section 4: Khắc phục sự cố (Troubleshooting)

#### Lỗi: ERR_CONNECTION_REFUSED
- **Nguyên nhân:** Không có gì lắng nghe ở cổng 80. Thường là do container `gateway-api` không chạy hoặc chạy sai lệnh.
- **Cách giải quyết:** Chạy `docker ps -a` trên EC2. Kiểm tra cột `STATUS` (phải là `Up`) và cột `PORTS` (phải là `0.0.0.0:80->5000/tcp`). Nếu sai, hãy chạy lại đúng lệnh ở **Section 3, Bước 4**.

#### Lỗi: 404 Not Found (trên Gateway)
- **Nguyên nhân:** Gateway không tìm thấy route nào khớp với đường dẫn bạn truy cập. Thường là do sai cấu hình trong file `ocelot.Production.json`.
- **Cách giải quyết:** Đảm bảo `ocelot.Production.json` chứa đầy đủ các route cần thiết và `DownstreamHostAndPorts` sử dụng đúng tên container (ví dụ: `authentication-api`).

#### Lỗi: Swagger UI không hoạt động đúng sau Gateway
- **Vấn đề 1: Trang Swagger không tải (404).**
  - **Nguyên nhân:** Gateway không có route cho trang Swagger.
  - **Cách giải quyết:** Đảm bảo `ocelot.Production.json` có route cho service đó (ví dụ: `/auth/{everything}`).
- **Vấn đề 2: Trang tải được nhưng khi "Execute" bị lỗi 404.**
  - **Nguyên nhân:** Swagger UI không biết thêm tiền tố của gateway (`/auth`) vào request.
  - **Cách giải quyết:** Sửa file `Program.cs` của microservice (ví dụ: `AuthenticationAPI`), thêm cấu hình `Server` vào `AddSwaggerGen`:
    ```csharp
    builder.Services.AddSwaggerGen(c =>
    {
        // ... các cấu hình khác ...
        c.AddServer(new OpenApiServer { Url = "/auth", Description = "API Gateway" });
    });
    ```
  - Sau đó build và deploy lại microservice đó.

#### Lỗi: "Invalid credentials"
- **Nguyên nhân:** Đây là lỗi logic của ứng dụng, không phải lỗi hệ thống. Nó có nghĩa là email hoặc mật khẩu không đúng với dữ liệu trong database.
- **Cách giải quyết:** Kiểm tra lại dữ liệu. Cách dễ nhất là dùng endpoint `register` để tạo một tài khoản mới và dùng chính tài khoản đó để đăng nhập.
