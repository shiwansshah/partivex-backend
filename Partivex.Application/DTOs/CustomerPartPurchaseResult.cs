namespace Partivex.Application.DTOs;

public sealed record CustomerPartPurchaseError(string Code, string Description);

public sealed class CustomerPartPurchaseResult<T>
{
    private CustomerPartPurchaseResult(T? value, IReadOnlyCollection<CustomerPartPurchaseError> errors, bool isNotFound, string? message)
    {
        Value = value;
        Errors = errors;
        IsNotFound = isNotFound;
        Message = message;
    }

    public T? Value { get; }

    public IReadOnlyCollection<CustomerPartPurchaseError> Errors { get; }

    public bool IsNotFound { get; }

    public string? Message { get; }

    public bool Succeeded => !IsNotFound && Errors.Count == 0 && Value is not null;

    public static CustomerPartPurchaseResult<T> Success(T value) => new(value, [], false, null);

    public static CustomerPartPurchaseResult<T> Failed(IReadOnlyCollection<CustomerPartPurchaseError> errors, string? message = null) =>
        new(default, errors, false, message);

    public static CustomerPartPurchaseResult<T> NotFound(string message) => new(default, [], true, message);
}
