using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;

/// <summary>
/// Mesmas regras do cadastro dos Obreiros: nome único sem diferenciar acento e
/// maiúscula, e nome removido é reaproveitado.
/// </summary>
public class CriarItemSocialCommandHandler(IAppDbContext db)
    : IRequestHandler<CriarItemSocialCommand, ItemSocialCriadoResult>
{
    private readonly IAppDbContext _db = db;

    public async Task<ItemSocialCriadoResult> Handle(CriarItemSocialCommand request, CancellationToken ct)
    {
        var normalizado = Item.Normalizar(request.Nome);

        var existente = await _db.ItensSociais
            .FirstOrDefaultAsync(i => i.NomeNormalizado == normalizado, ct);

        if (existente is not null)
        {
            if (existente.Ativo)
                throw new ConflitoException("Já existe um item com esse nome");

            existente.Reativar(request.Nome, request.Unidade);

            await _db.SaveChangesAsync(ct);

            return new ItemSocialCriadoResult(
                existente.Id, existente.Nome, existente.Unidade, existente.EstoqueAtual);
        }

        var item = ItemSocial.Criar(request.Nome, request.Unidade);

        _db.ItensSociais.Add(item);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (await _db.ItensSociais.AnyAsync(i => i.NomeNormalizado == normalizado && i.Id != item.Id, ct))
                throw new ConflitoException("Já existe um item com esse nome");

            throw;
        }

        return new ItemSocialCriadoResult(item.Id, item.Nome, item.Unidade, item.EstoqueAtual);
    }
}
