namespace Partivex.Application.DTOs;

public sealed record LoginCommand(string Email, string Password);

public sealed record RegisterCommand(string FullName, string Email, string Password);

public sealed record CreateUserCommand(string FullName, string Email, string Password, string Role);

public sealed record AuthResponse(string Token);

public sealed record UserCreatedResponse(string UserId, string Email, string Role);

public sealed record AuthError(string Code, string Description);

public sealed class AuthResult<T>
{
    private AuthResult(T? value, IReadOnlyCollection<AuthError> errors, bool isUnauthorized)
    {
        Value = value;
        Errors = errors;
        IsUnauthorized = isUnauthorized;
    }

    public T? Value { get; }

    public IReadOnlyCollection<AuthError> Errors { get; }

    public bool Succeeded => Value is not null && Errors.Count == 0 && !IsUnauthorized;

    public bool IsUnauthorized { get; }

    public static AuthResult<T> Success(T value) => new(value, [], false);

    public static AuthResult<T> Failed(IReadOnlyCollection<AuthError> errors) => new(default, errors, false);

    public static AuthResult<T> Unauthorized() => new(default, [], true);
}
