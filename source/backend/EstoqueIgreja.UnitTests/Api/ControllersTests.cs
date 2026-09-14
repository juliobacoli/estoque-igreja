using EstoqueIgreja.Api.Controllers;
using EstoqueIgreja.Application.Auth.Commands.Login;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EstoqueIgreja.UnitTests.Api;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_CredenciaisInvalidas_Retorna401ComMensagemGenerica()
    {
        var mediator = new Mock<ISender>();
        mediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginResult?)null);

        var resposta = await new AuthController(mediator.Object)
            .Login(new LoginCommand("admin", "errada"), CancellationToken.None);

        var naoAutorizado = Assert.IsType<UnauthorizedObjectResult>(resposta);
        Assert.Equal("Login ou senha inválidos", Reflexao.Ler(naoAutorizado.Value!, "error"));
    }
}

public class HealthControllerTests
{
    private const string Segredo = "Host=db-interno;Password=senha-secreta";

    private sealed class ContextoQueFalha(DbContextOptions<AppDbContext> opcoes) : AppDbContext(opcoes)
    {
        public override DatabaseFacade Database => throw new InvalidOperationException(Segredo);
    }

    [Fact]
    public async Task GetDb_Excecao_Retorna503SemDetalhesDoErro()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var controller = new HealthController(new ContextoQueFalha(opcoes), NullLogger<HealthController>.Instance);

        var resposta = Assert.IsType<ObjectResult>(await controller.GetDb());

        Assert.Equal(503, resposta.StatusCode);
        Assert.Equal("não foi possível conectar ao banco", Reflexao.Ler(resposta.Value!, "error"));
        Assert.DoesNotContain("senha-secreta", System.Text.Json.JsonSerializer.Serialize(resposta.Value));
    }
}
