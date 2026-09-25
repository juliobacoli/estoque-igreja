using FluentValidation;
using MediatR;

namespace EstoqueIgreja.Application.Common.Behaviors;

/// <summary>
/// Roda os validators do FluentValidation antes do Handler. Se algum falhar, o
/// Handler não chega a ser chamado.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var falhas = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken))))
            .SelectMany(resultado => resultado.Errors)
            .Where(falha => falha is not null)
            .ToList();

        if (falhas.Count != 0)
            throw new ValidationException(falhas);

        return await next(cancellationToken);
    }
}
