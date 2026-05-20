namespace Partivex.Application.DTOs;

public sealed record NotificationDto(
    int Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTimeOffset CreatedAt);
