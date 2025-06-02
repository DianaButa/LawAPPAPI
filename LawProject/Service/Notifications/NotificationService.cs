using LawProject.Database;
using LawProject.Models;
using Microsoft.EntityFrameworkCore;

namespace LawProject.Service.Notifications
{
  public class NotificationService: INotificationService
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
      try
      {
        // Validate notification
        if (notification == null)
        {
          throw new ArgumentNullException(nameof(notification));
        }

        if (string.IsNullOrEmpty(notification.Title))
        {
          throw new ArgumentException("Notification title is required");
        }

        if (string.IsNullOrEmpty(notification.Message))
        {
          throw new ArgumentException("Notification message is required");
        }

        if (notification.UserId <= 0)
        {
          throw new ArgumentException("Valid user ID is required");
        }

        // Set timestamp if not set
        if (notification.Timestamp == default)
        {
          notification.Timestamp = DateTime.UtcNow;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Created notification with ID: {notification.Id}");
        return notification;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error creating notification");
        throw;
      }
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId)
    {
      var notifications = await _context.Notifications
          .Where(n => n.UserId == userId)
          .OrderByDescending(n => n.Timestamp)
          .ToListAsync();

      return notifications;
    }


    public async Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type, int page = 1, int pageSize = 10)
    {
      try
      {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.Type == type)
            .OrderByDescending(n => n.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation($"Retrieved {notifications.Count} notifications of type {type} for user {userId}");
        return notifications;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving notifications of type {type} for user {userId}");
        throw;
      }
    }


    public async Task<List<Notification>> GetAllNotificationsAsync()
    {
      var notifications = await _context.Notifications
          .OrderByDescending(n => n.Timestamp)
          .ToListAsync();

      return notifications;
    }

    public async Task<List<Notification>> GetNotificationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
      try
      {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId &&
                       n.Timestamp >= startDate &&
                       n.Timestamp <= endDate)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();

        _logger.LogInformation($"Retrieved {notifications.Count} notifications between {startDate} and {endDate} for user {userId}");
        return notifications;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving notifications by date range for user {userId}");
        throw;
      }
    }

    public async Task<List<Notification>> GetNotificationsByFileNumberAsync(int userId, string fileNumber)
    {
      try
      {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.FileNumber == fileNumber)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();

        _logger.LogInformation($"Retrieved {notifications.Count} notifications for file {fileNumber} and user {userId}");
        return notifications;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving notifications for file {fileNumber} and user {userId}");
        throw;
      }
    }

    public async Task<List<Notification>> GetNotificationsByStatusAsync(int userId, bool isRead, int page = 1, int pageSize = 10)
    {
      try
      {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && n.IsRead == isRead)
            .OrderByDescending(n => n.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation($"Retrieved {notifications.Count} {(isRead ? "read" : "unread")} notifications for user {userId}");
        return notifications;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving {(isRead ? "read" : "unread")} notifications for user {userId}");
        throw;
      }
    }

    public async Task<int> GetUnreadNotificationsCountAsync(int userId)
    {
      try
      {
        var count = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

        _logger.LogInformation($"Retrieved {count} unread notifications for user {userId}");
        return count;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error retrieving unread notifications count for user {userId}");
        throw;
      }
    }

    public async Task<Notification> MarkAsReadAsync(int notificationId)
    {
      try
      {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
        {
          _logger.LogWarning($"Notification with ID {notificationId} not found");
          return null;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Marked notification {notificationId} as read");
        return notification;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error marking notification {notificationId} as read");
        throw;
      }
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
      try
      {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
          notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Marked all notifications as read for user {userId}");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error marking all notifications as read for user {userId}");
        throw;
      }
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
      try
      {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null)
        {
          _logger.LogWarning($"Notification with ID {notificationId} not found");
          return false;
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Deleted notification {notificationId}");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error deleting notification {notificationId}");
        throw;
      }
    }

    public async Task<bool> DeleteOldNotificationsAsync(int daysOld)
    {
      try
      {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await _context.Notifications
            .Where(n => n.Timestamp < cutoffDate)
            .ToListAsync();

        if (!oldNotifications.Any())
        {
          _logger.LogInformation($"No notifications older than {daysOld} days found");
          return true;
        }

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Deleted {oldNotifications.Count} notifications older than {daysOld} days");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error deleting notifications older than {daysOld} days");
        throw;
      }
    }
  }
}

