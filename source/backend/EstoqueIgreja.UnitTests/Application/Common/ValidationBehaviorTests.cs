using EstoqueIgreja.Application.Common.Behaviors;
using FluentValidation;
using MediatR;

namespace EstoqueIgreja.UnitTests.Application.Common;

public class ValidationBehaviorTests
{
    public record RequisicaoTeste(string Valor) : IRequest<string>;

    private bool _proximoChamado;

    private Task<string> Proximo()
    {
        _proximoChamado = true;
        return Task.FromResult("ok");
    }

    private static InlineValidator<RequisicaoTeste> ValidadorExigindo(string mensagem) => new()
    {
        v => v.RuleFor(x => x.Valor).NotEmpty().WithMessage(mensagem)
    };

    private Task<string> Executar(RequisicaoTeste requisicao, params IValidator<RequisicaoTeste>[] validators) =>
        new ValidationBehavior<RequisicaoTeste, string>(validators)
            .Handle(requisicao, _ => Proximo(), CancellationToken.None);

    [Fact]
    public async Task SemValidators_ChamaProximo()
    {
        var resultado = await Executar(new RequisicaoTeste(""));

        Assert.True(_proximoChamado);
        Assert.Equal("ok", resultado);
    }

    [Fact]
    public async Task ValidatorSemFalha_ChamaProximo()
    {
        await Executar(new RequisicaoTeste("valor"), ValidadorExigindo("erro"));

        Assert.True(_proximoChamado);
    }

    [Fact]
    public async Task ValidatorComFalha_LancaExcecaoSemChamarProximo()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => Executar(new RequisicaoTeste(""), ValidadorExigindo("erro")));

        Assert.False(_proximoChamado);
    }

    [Fact]
    public async Task VariosValidators_JuntaFalhasDeTodos()
    {
        var excecao = await Assert.ThrowsAsync<ValidationException>(
            () => Executar(new RequisicaoTeste(""), ValidadorExigindo("primeiro"), ValidadorExigindo("segundo")));

        Assert.Equal(["primeiro", "segundo"], excecao.Errors.Select(e => e.ErrorMessage).Order());
    }
}
