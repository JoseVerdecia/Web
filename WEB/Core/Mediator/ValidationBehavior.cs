using FluentValidation;
using WEB.Common;
using WEB.Core.Result;
using WEB.Enums;

namespace WEB.Core.Mediator;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators ?? throw new ArgumentNullException(nameof(validators));
    }

    public async Task<AppResult<TResponse>> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .Select(f => new ErrorDetail
            {
                Field = string.IsNullOrWhiteSpace(f.PropertyName) ? null : f.PropertyName,
                Message = f.ErrorMessage,
                Type = ErrorType.Validation
            })
            .ToList();

        if (failures.Any())
            return AppResult<TResponse>.Fail(failures);

        return await next();
    }
}