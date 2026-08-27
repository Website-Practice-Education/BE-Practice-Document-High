using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(long userId, string title, string content);
    Task<List<Notification>> GetUserNotificationsAsync(long userId, int page = 1, int pageSize = 20);
    Task<List<Notification>> GetUnreadNotificationsAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
    Task MarkAsReadAsync(long notificationId);
    Task MarkAllAsReadAsync(long userId);
}

public class NotificationDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
