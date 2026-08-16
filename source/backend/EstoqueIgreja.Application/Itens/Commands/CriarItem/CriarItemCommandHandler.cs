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

        var jaExiste = await _db.Itens.AnyAsync(i => i.NomeNormalizado == normalizado, ct);

        if (jaExiste)
        {
            throw new ConflitoException("Já existe um item com esse nome");
        }

        var item = Item.Criar(request.Nome, request.Unidade);

        _db.Itens.Add(item);
        await _db.SaveChangesAsync(ct);

        return new ItemCriadoResult(item.Id, item.Nome, item.Unidade, item.EstoqueAtual);
    }
}
