using LawProject.Models;
using LawProject.Service.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LawProject.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationsController : ControllerBase
  {
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
      _notificationService = notificationService;
      _logger = logger;
    }



    [HttpGet("all")]
    public async Task<IActionResult> GetAllNotifications()
    {
      var notifications = await _notificationService.GetAllNotificationsAsync();
      return Ok(notifications);
    }
    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetUserNotifications()
    {
      try
      {
        // TODO: Get actual user ID from authentication
        int userId = 1; // Temporary hardcoded user ID
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(notifications);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving notifications");
        return StatusCode(500, "Error retrieving notifications");
      }
    }

    [HttpGet("unread/count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
      try
      {
        // TODO: Get actual user ID from authentication
        int userId = 1; // Temporary hardcoded user ID
        var count = await _notificationService.GetUnreadNotificationsCountAsync(userId);
        return Ok(count);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error retrieving unread notifications count");
        return StatusCode(500, "Error retrieving unread notifications count");
      }
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<Notification>> MarkAsRead(int id)
    {
      try
      {
        var notification = await _notificationService.MarkAsReadAsync(id);
        if (notification == null)
        {
          return NotFound($"Notification with ID {id} not found");
        }
        return Ok(notification);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error marking notification {id} as read");
        return StatusCode(500, "Error marking notification as read");
      }
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
      try
      {
        // TODO: Get actual user ID from authentication
        int userId = 1; // Temporary hardcoded user ID
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error marking all notifications as read");
        return StatusCode(500, "Error marking all notifications as read");
      }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(int id)
    {
      try
      {
        var result = await _notificationService.DeleteNotificationAsync(id);
        if (!result)
        {
          return NotFound($"Notification with ID {id} not found");
        }
        return Ok();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error deleting notification {id}");
        return StatusCode(500, "Error deleting notification");
      }
    }
  }
} 
