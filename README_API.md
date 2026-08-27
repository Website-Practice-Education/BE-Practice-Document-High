# Hướng Dẫn Sử Dụng Website_Documents API

## Cấu Trúc Dự Án

```
Practice_Information_Document/
├── Website_Documents.API/           # Web API
│   ├── Controllers/                 # API Controllers
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Middleware/                  # Custom Middleware
│   └── Program.cs                   # Entry point
├── Website_Documents.Service/       # Business Logic Layer
│   ├── Interfaces/                  # Service Interfaces
│   └── Services/                    # Service Implementations
├── Website_Documents.Repository/     # Data Access Layer
│   ├── DBContext/                   # Entity Framework Context
│   ├── Interfaces/                  # Repository Interfaces
│   ├── Models/                      # Entity Models
│   └── Repositories/                # Repository Implementations
└── SQL/                             # Database Scripts
    └── init_database.sql            # Khởi tạo database
```

## Cài Đặt

### 1. Cấu Hình Database

1. Chạy SQL script `SQL/init_database.sql` trong PostgreSQL (pgAdmin)
2. Cập nhật `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bookstore_db;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "WebsiteDocumentsAPI",
    "Audience": "WebsiteDocumentsClient",
    "ExpiryDays": 7
  }
}
```

### 2. Build và Chạy

```bash
cd Website_Documents.API
dotnet run
```

API sẽ chạy tại: `http://localhost:5000` hoặc `https://localhost:5001`

Swagger UI: `http://localhost:5000/swagger`

---

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| POST | `/api/auth/register` | Đăng ký tài khoản mới | No |
| POST | `/api/auth/login` | Đăng nhập | No |
| POST | `/api/auth/change-password` | Đổi mật khẩu | Yes |
| PUT | `/api/auth/profile` | Cập nhật profile | Yes |

**Ví dụ Register:**
```json
{
  "email": "student@example.com",
  "password": "123456",
  "fullName": "Nguyễn Văn A",
  "grade": 10
}
```

**Ví dụ Login Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "student@example.com",
    "fullName": "Nguyễn Văn A",
    "role": "student",
    "expiresAt": "2026-09-03T08:00:00Z"
  }
}
```

---

### Users (`/api/users`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/users` | Lấy danh sách users | Yes |
| GET | `/api/users/{id}` | Lấy user theo ID | Yes |
| GET | `/api/users/email/{email}` | Lấy user theo email | Yes |
| POST | `/api/users` | Tạo user mới | Yes |
| PUT | `/api/users/{id}` | Cập nhật user | Yes |
| DELETE | `/api/users/{id}` | Xóa user | Yes |

---

### Subjects (`/api/subjects`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/subjects` | Lấy danh sách môn học | No |
| GET | `/api/subjects/{id}` | Lấy môn học theo ID | No |
| POST | `/api/subjects` | Tạo môn học mới | Yes |
| PUT | `/api/subjects/{id}` | Cập nhật môn học | Yes |
| DELETE | `/api/subjects/{id}` | Xóa môn học | Yes |

---

### Exams (`/api/exams`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/exams` | Lấy danh sách đề thi | No |
| GET | `/api/exams/{id}` | Lấy đề thi theo ID | No |
| GET | `/api/exams/subject/{subjectId}` | Lấy đề thi theo môn | No |
| POST | `/api/exams` | Tạo đề thi mới | Yes |
| PUT | `/api/exams/{id}` | Cập nhật đề thi | Yes |
| DELETE | `/api/exams/{id}` | Xóa đề thi | Yes |

---

### Questions (`/api/questions`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/questions` | Lấy danh sách câu hỏi | No |
| GET | `/api/questions/{id}` | Lấy câu hỏi theo ID | No |
| GET | `/api/questions/subject/{subjectId}` | Lấy câu hỏi theo môn | No |
| GET | `/api/questions/lesson/{lessonId}` | Lấy câu hỏi theo bài | No |
| POST | `/api/questions` | Tạo câu hỏi mới | Yes |
| PUT | `/api/questions/{id}` | Cập nhật câu hỏi | Yes |
| DELETE | `/api/questions/{id}` | Xóa câu hỏi | Yes |

---

### Exam Attempts (`/api/examattempts`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| POST | `/api/examattempts/start/{examId}` | Bắt đầu thi | Yes |
| POST | `/api/examattempts/{attemptId}/answer` | Nộp câu trả lời | Yes |
| POST | `/api/examattempts/{attemptId}/submit` | Nộp bài thi | Yes |
| GET | `/api/examattempts/{attemptId}` | Lấy thông tin lượt thi | Yes |
| GET | `/api/examattempts/{attemptId}/result` | Lấy kết quả thi | Yes |
| GET | `/api/examattempts/my-attempts` | Lấy lịch sử thi của tôi | Yes |

**Ví dụ Submit Answer:**
```json
{
  "questionId": 1,
  "selectedOptionId": 3
}
```

---

### Progress (`/api/progress`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/progress/dashboard` | Lấy dashboard | Yes |
| GET | `/api/progress/today` | Tiến độ hôm nay | Yes |
| GET | `/api/progress/weekly` | Tiến độ tuần này | Yes |
| POST | `/api/progress/lesson/{lessonId}` | Cập nhật tiến độ bài | Yes |
| POST | `/api/progress/topic/{topicId}` | Cập nhật tiến độ chủ đề | Yes |

---

### Notifications (`/api/notifications`)

| Method | Endpoint | Mô tả | Auth |
|--------|----------|--------|------|
| GET | `/api/notifications` | Lấy thông báo | Yes |
| GET | `/api/notifications/unread` | Thông báo chưa đọc | Yes |
| GET | `/api/notifications/unread-count` | Số thông báo chưa đọc | Yes |
| POST | `/api/notifications/{id}/read` | Đánh dấu đã đọc | Yes |
| POST | `/api/notifications/read-all` | Đánh dấu tất cả đã đọc | Yes |

---

## Sử Dụng JWT Token

Sau khi đăng nhập, thêm header vào các request cần auth:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Response Format

**Thành công:**
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... }
}
```

**Lỗi:**
```json
{
  "success": false,
  "message": "Error message",
  "errors": []
}
```

---

## Ví dụ sử dụng với cURL

### Đăng ký
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"123456","fullName":"Test User"}'
```

### Đăng nhập
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"123456"}'
```

### Lấy danh sách môn học
```bash
curl http://localhost:5000/api/subjects
```

### Tạo đề thi (có auth)
```bash
curl -X POST http://localhost:5000/api/exams \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{"title":"Đề thi Toán 2026","subjectId":1,"durationMinutes":60}'
```

---

## Các Role

- `admin` - Quản trị viên
- `student` - Học sinh
- `teacher` - Giáo viên

---

## Error Codes

| HTTP Status | Mô tả |
|-------------|--------|
| 200 | Thành công |
| 400 | Bad Request - Dữ liệu không hợp lệ |
| 401 | Unauthorized - Chưa đăng nhập |
| 403 | Forbidden - Không có quyền |
| 404 | Not Found - Không tìm thấy |
| 500 | Internal Server Error - Lỗi server |
