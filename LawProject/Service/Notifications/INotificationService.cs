

using LawProject.Models;

namespace LawProject.Service.Notifications
{
  public interface INotificationService
  {
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task<List<Notification>> GetUserNotificationsAsync(int userId);
    Task<List<Notification>> GetAllNotificationsAsync();
    Task<List<Notification>> GetNotificationsByTypeAsync(int userId, string type, int page = 1, int pageSize = 10);
    Task<List<Notification>> GetNotificationsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<List<Notification>> GetNotificationsByFileNumberAsync(int userId, string fileNumber);
    Task<List<Notification>> GetNotificationsByStatusAsync(int userId, bool isRead, int page = 1, int pageSize = 10);
    Task<int> GetUnreadNotificationsCountAsync(int userId);
    Task<Notification> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<bool> DeleteNotificationAsync(int notificationId);
    Task<bool> DeleteOldNotificationsAsync(int daysOld);
    Task<bool> NotificationExistsForFileAndDateAsync(string fileNumber, DateTime date);

  }
}
