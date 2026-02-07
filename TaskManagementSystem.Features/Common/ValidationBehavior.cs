using FluentValidation;
using MediatR;
using TaskManagementSystem.Domain.Common;

namespace TaskManagementSystem.Features.Common;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before the handler executes.
/// Returns a failed Result with validation errors instead of throwing exceptions.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse);
            var failureMethod = resultType.GetMethod("Failure", new[] { typeof(Error) })!;
            return (TResponse)failureMethod.Invoke(null, new object[] { Error.Validation(errorMessage) })!;
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(Error.Validation(errorMessage));
        }

        throw new ValidationException(failures);
    }
}
