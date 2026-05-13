namespace Partivex.Application.Interfaces;

public interface ICurrentUserContext
{
    string UserId { get; }

    string UserName { get; }

    string Role { get; }

    string IpAddress { get; }
}
