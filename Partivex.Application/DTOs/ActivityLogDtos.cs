namespace Partivex.Application.DTOs;

public sealed record ActivityLogDto(
    int Id,
    string UserId,
    string UserName,
    string Role,
    string Action,
    string EntityName,
    string EntityId,
    string Description,
    string IpAddress,
    DateTimeOffset CreatedAt);

public sealed record ActivityLogFilterDto(
    string? User,
    string? Action,
    DateTimeOffset? From,
    DateTimeOffset? To);

public sealed record CreateActivityLogCommand(
    string Action,
    string EntityName,
    string EntityId,
    string Description);
