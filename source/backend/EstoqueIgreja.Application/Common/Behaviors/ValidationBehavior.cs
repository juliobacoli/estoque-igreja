using FluentValidation;
using MediatR;

namespace EstoqueIgreja.Application.Common.Behaviors;

/// <summary>
/// Roda os validators do FluentValidation antes do Handler. Se algum falhar, o
/// Handler não chega a ser chamado.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var contexto = new ValidationContext<TRequest>(request);

        var falhas = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(contexto, cancellationToken))))
            .SelectMany(resultado => resultado.Errors)
            .Where(falha => falha is not null)
            .ToList();

        if (falhas.Count != 0)
        {
            throw new ValidationException(falhas);
        }

        return await next();
    }
}
