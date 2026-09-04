using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;

namespace Website_Documents.API.Data
{
    public static class DocumentSeeder
    {
        public static async Task SeedDocumentsAsync(BookstoreDbContext context)
        {
            // Check if documents already exist
            if (await context.SharedDocuments.AnyAsync())
            {
                Console.WriteLine("Documents already exist, skipping seed.");
                return;
            }

            // Get a user ID for seeding
            var user = await context.Users.FirstOrDefaultAsync(u => u.Role == "teacher");
            var userId = user?.Id ?? 1;

            var documents = new List<SharedDocument>
            {
                // Approved documents
                new SharedDocument
                {
                    Title = "Đề thi Toán THPT Quốc gia 2024 - Đợt 1",
                    Description = "Đề thi chính thức môn Toán kỳ thi THPT Quốc gia 2024, đợt 1. Bao gồm đáp án chi tiết.",
                    DocumentType = "link",
                    LinkUrl = "https://drive.google.com/example1",
                    LinkSource = "Google Drive",
                    SubjectId = 1, // Assuming Math
                    QuestionCount = 50,
                    GradeLevel = 12,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ViewCount = 1520,
                    DownloadCount = 890,
                    LikeCount = 245,
                    IsActive = true,
                    IsVerified = true,
                    ModerationStatus = "approved",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new SharedDocument
                {
                    Title = "Tổng hợp công thức Vật lý lớp 12",
                    Description = "Bảng tổng hợp tất cả các công thức Vật lý lớp 12 theo chương trình mới. File PDF có thể in A4.",
                    DocumentType = "file",
                    FileUrl = "https://example-storage.com/docs/cong-thuc-vat-ly-12.pdf",
                    FileType = "application/pdf",
                    FileSize = 2048000,
                    SubjectId = 2, // Physics
                    QuestionCount = null,
                    GradeLevel = 12,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ViewCount = 3420,
                    DownloadCount = 1560,
                    LikeCount = 520,
                    IsActive = true,
                    IsVerified = true,
                    ModerationStatus = "approved",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-8),
                    CreatedAt = DateTime.UtcNow.AddDays(-12),
                    UpdatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new SharedDocument
                {
                    Title = "Bài tập Hóa học hữu cơ lớp 11 - Có lời giải",
                    Description = "Tuyển tập 200 bài tập Hóa học hữu cơ lớp 11 kèm lời giải chi tiết. Phù hợp cho học sinh ôn tập.",
                    DocumentType = "link",
                    LinkUrl = "https://zalo.me/example2",
                    LinkSource = "Zalo",
                    SubjectId = 3, // Chemistry
                    QuestionCount = 200,
                    GradeLevel = 11,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ViewCount = 890,
                    DownloadCount = 450,
                    LikeCount = 156,
                    IsActive = true,
                    IsVerified = true,
                    ModerationStatus = "approved",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new SharedDocument
                {
                    Title = "Ngữ văn 12: Phân tích tác phẩm 'Truyện Kiều'",
                    Description = "Tài liệu hướng dẫn phân tích các đoạn trích tiêu biểu trong Truyện Kiều của Nguyễn Du.",
                    DocumentType = "link",
                    LinkUrl = "https://drive.google.com/example3",
                    LinkSource = "Google Drive",
                    SubjectId = 4, // Literature
                    GradeLevel = 12,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ViewCount = 2340,
                    DownloadCount = 1100,
                    LikeCount = 380,
                    IsActive = true,
                    IsVerified = true,
                    ModerationStatus = "approved",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-3),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new SharedDocument
                {
                    Title = "Đề thi thử Tiếng Anh IELTS Reading Practice",
                    Description = "Bộ 5 đề thi thử IELTS Reading kèm đáp án và giải thích chi tiết. Độ khó tương đương band 6.5-7.5.",
                    DocumentType = "file",
                    FileUrl = "https://example-storage.com/docs/ielts-reading.pdf",
                    FileType = "application/pdf",
                    FileSize = 3584000,
                    SubjectId = 5, // English
                    GradeLevel = 12,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ViewCount = 4560,
                    DownloadCount = 2100,
                    LikeCount = 680,
                    IsActive = true,
                    IsVerified = true,
                    ModerationStatus = "approved",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-4),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                // Pending documents (for testing moderation)
                new SharedDocument
                {
                    Title = "Tài liệu Toán lớp 10 - Chương trình mới",
                    Description = "Bài tập Toán lớp 10 theo chương trình GDPT 2018.",
                    DocumentType = "link",
                    LinkUrl = "https://example.com/toan-10",
                    SubjectId = 1,
                    QuestionCount = 100,
                    GradeLevel = 10,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ModerationStatus = "pending",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new SharedDocument
                {
                    Title = "Đề kiểm tra 1 tiết Vật lý lớp 11",
                    Description = "Đề kiểm tra 45 phút chương Dao động cơ học.",
                    DocumentType = "file",
                    FileUrl = "https://example-storage.com/docs/vat-ly-11-kt.pdf",
                    FileType = "application/pdf",
                    FileSize = 512000,
                    SubjectId = 2,
                    QuestionCount = 20,
                    GradeLevel = 11,
                    SharedByUserId = userId,
                    SharedByName = user?.FullName ?? "Giáo viên",
                    ModerationStatus = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Rejected document
                new SharedDocument
                {
                    Title = "Test tài liệu - Đã bị từ chối",
                    Description = "Tài liệu này bị từ chối vì không phù hợp nội dung.",
                    DocumentType = "link",
                    LinkUrl = "https://example.com/test",
                    ModerationStatus = "rejected",
                    ModerationNotes = "Nội dung không phù hợp với tiêu chuẩn của trang.",
                    ModeratedByUserId = 1,
                    ModeratedByName = "Quản trị viên",
                    ModeratedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            await context.SharedDocuments.AddRangeAsync(documents);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {documents.Count} documents!");
        }
    }
}
