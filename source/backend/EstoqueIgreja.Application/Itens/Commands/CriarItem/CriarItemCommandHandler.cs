using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Commands.CriarItem;

public class CriarItemCommandHandler(IAppDbContext db) : IRequestHandler<CriarItemCommand, ItemCriadoResult>
{
    private readonly IAppDbContext _db = db;

    public async Task<ItemCriadoResult> Handle(CriarItemCommand request, CancellationToken ct)
    {
        var normalizado = Item.Normalizar(request.Nome);

        var existente = await _db.Itens
            .FirstOrDefaultAsync(i => i.NomeNormalizado == normalizado, ct);

        if (existente is not null)
        {
            if (existente.Ativo)
                throw new ConflitoException("Já existe um item com esse nome");

            existente.Reativar(request.Nome, request.Unidade);

            await _db.SaveChangesAsync(ct);

            return new ItemCriadoResult(
                existente.Id, existente.Nome, existente.Unidade, existente.EstoqueAtual);
        }

        var item = Item.Criar(request.Nome, request.Unidade);

        _db.Itens.Add(item);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await _db.Itens.AnyAsync(i => i.NomeNormalizado == normalizado && i.Id != item.Id, ct))
                throw new ConflitoException("Já existe um item com esse nome");

            throw;
        }

        return new ItemCriadoResult(item.Id, item.Nome, item.Unidade, item.EstoqueAtual);
    }
}
