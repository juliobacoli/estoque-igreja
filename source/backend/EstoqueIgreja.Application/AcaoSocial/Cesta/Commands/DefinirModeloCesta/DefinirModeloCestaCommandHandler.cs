using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.DefinirModeloCesta;

public class DefinirModeloCestaCommandHandler : IRequestHandler<DefinirModeloCestaCommand, Unit>
{
    private readonly IAppDbContext _db;

    public DefinirModeloCestaCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DefinirModeloCestaCommand request, CancellationToken ct)
    {
        var ids = request.Itens.Select(i => i.ItemId).ToList();

        var ativos = await _db.ItensSociais
            .CountAsync(i => ids.Contains(i.Id) && i.Ativo, ct);

        if (ativos != ids.Count)
            throw new NaoEncontradoException("Item não encontrado");

        var modelo = await _db.ModelosCesta
            .Include(m => m.Itens)
            .FirstOrDefaultAsync(m => m.Id == ModeloCesta.IdUnico, ct);

        if (modelo is null)
        {
            modelo = ModeloCesta.Criar();
            _db.ModelosCesta.Add(modelo);
        }

        modelo.DefinirItens(request.Itens.Select(i => (i.ItemId, i.Quantidade)));

        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
