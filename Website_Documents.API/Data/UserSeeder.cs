using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;

namespace Website_Documents.API.Data
{
    public static class UserSeeder
    {
        public static async Task SeedUsersAsync(BookstoreDbContext context)
        {
            // Check if users already exist
            if (await context.Users.AnyAsync())
            {
                Console.WriteLine("Users already exist, skipping seed.");
                return;
            }

            var users = new List<User>
            {
                new User
                {
                    Email = "admin@example.com",
                    PasswordHash = HashPassword("admin123"),
                    FullName = "Quản trị viên",
                    Role = "admin",
                    Grade = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "teacher@example.com",
                    PasswordHash = HashPassword("teacher123"),
                    FullName = "Nguyễn Văn Giáo Viên",
                    Role = "teacher",
                    Grade = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "teacher2@example.com",
                    PasswordHash = HashPassword("teacher123"),
                    FullName = "Trần Thị Giáo Viên",
                    Role = "teacher",
                    Grade = 11,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "student@example.com",
                    PasswordHash = HashPassword("student123"),
                    FullName = "Lê Minh Học Sinh",
                    Role = "student",
                    Grade = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "student2@example.com",
                    PasswordHash = HashPassword("student123"),
                    FullName = "Phạm Thị Học Sinh",
                    Role = "student",
                    Grade = 11,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "student3@example.com",
                    PasswordHash = HashPassword("student123"),
                    FullName = "Hoàng Văn Học Sinh",
                    Role = "student",
                    Grade = 12,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "inactive@example.com",
                    PasswordHash = HashPassword("test123"),
                    FullName = "User Ngừng Hoạt Động",
                    Role = "student",
                    Grade = 10,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "john@example.com",
                    PasswordHash = HashPassword("test123"),
                    FullName = "John Smith",
                    Role = "student",
                    Grade = 10,
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "emma@example.com",
                    PasswordHash = HashPassword("test123"),
                    FullName = "Emma Watson",
                    Role = "student",
                    Grade = 11,
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                    UpdatedAt = DateTime.UtcNow
                },
                new User
                {
                    Email = "michael@example.com",
                    PasswordHash = HashPassword("test123"),
                    FullName = "Michael Chen",
                    Role = "student",
                    Grade = 12,
                    IsActive = true,
                    LastLoginAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            Console.WriteLine($"Successfully seeded {users.Count} users!");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
