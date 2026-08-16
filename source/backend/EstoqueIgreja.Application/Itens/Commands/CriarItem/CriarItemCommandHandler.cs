using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Commands.CriarItem;

public class CriarItemCommandHandler : IRequestHandler<CriarItemCommand, ItemCriadoResult>
{
    private readonly IAppDbContext _db;

    public CriarItemCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<ItemCriadoResult> Handle(CriarItemCommand request, CancellationToken ct)
    {
        var normalizado = Item.Normalizar(request.Nome);

        var existente = await _db.Itens
            .FirstOrDefaultAsync(i => i.NomeNormalizado == normalizado, ct);

        if (existente is not null)
        {
            // Nome de item ativo é duplicata de verdade.
            if (existente.Ativo)
            {
                throw new ConflitoException("Já existe um item com esse nome");
            }

            // Nome de item inativo reativa o registro em vez de criar outro: o
            // índice único continua valendo e o histórico antigo segue ligado a
            // este mesmo item.
            existente.Reativar(request.Nome, request.Unidade);

            await _db.SaveChangesAsync(ct);

            return new ItemCriadoResult(
                existente.Id, existente.Nome, existente.Unidade, existente.EstoqueAtual);
        }

        var item = Item.Criar(request.Nome, request.Unidade);

        _db.Itens.Add(item);
        await _db.SaveChangesAsync(ct);

        return new ItemCriadoResult(item.Id, item.Nome, item.Unidade, item.EstoqueAtual);
    }
}
