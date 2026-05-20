using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var (role, userId) = GetUserContext();
        var notifications = await _notificationService.GetForUserAsync(role, userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var (role, userId) = GetUserContext();
        var count = await _notificationService.GetUnreadCountAsync(role, userId, cancellationToken);
        return Ok(count);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    private (string role, string? userId) GetUserContext()
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
        var role = roles.FirstOrDefault() ?? string.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return (role, userId);
    }
}
