using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Estoque.Queries.ObterUltimaAtualizacao;

public class ObterUltimaAtualizacaoQueryHandler : IRequestHandler<ObterUltimaAtualizacaoQuery, UltimaAtualizacaoResult>
{
    private readonly IAppDbContext _db;

    public ObterUltimaAtualizacaoQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<UltimaAtualizacaoResult> Handle(ObterUltimaAtualizacaoQuery request, CancellationToken ct)
    {
        // O cast para DateTime? faz o MAX de uma tabela vazia voltar null em vez de lançar.
        var data = await _db.AtualizacoesEstoque.MaxAsync(a => (DateTime?)a.Data, ct);

        return new UltimaAtualizacaoResult(data);
    }
}
