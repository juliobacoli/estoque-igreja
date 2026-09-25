using EstoqueIgreja.Application.AcaoSocial.Cesta.Common;
using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;

public class MontarCestasCommandHandler(IAppDbContext db, IUsuarioAtual usuarioAtual)
    : IRequestHandler<MontarCestasCommand, CestasMontadasResult>
{
    private readonly IAppDbContext _db = db;
    private readonly IUsuarioAtual _usuarioAtual = usuarioAtual;

    public async Task<CestasMontadasResult> Handle(MontarCestasCommand request, CancellationToken ct)
    {
        using var transaction = await _db.BeginTransactionAsync(ct);

        // Trava o modelo primeiro: duas montagens ao mesmo tempo entram em fila aqui,
        // e a segunda só lê o estoque depois que a primeira gravou.
        var modelo = await _db.ModelosCesta
            .FromSqlInterpolated($"SELECT * FROM \"ModelosCesta\" WHERE \"Id\" = {ModeloCesta.IdUnico} FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        var composicao = modelo is null
            ? []
            : await _db.ModeloCestaItens.AsNoTracking().Where(i => i.ModeloCestaId == modelo.Id).ToListAsync(ct);

        if (modelo is null || composicao.Count == 0)
            throw new ConflitoException("Defina o modelo da cesta antes de montar.");

        // Doação e ajuste travam só o item; a montagem trava todos os itens da cesta,
        // sempre na mesma ordem (por Id) para duas transações não se esperarem em círculo.
        var ids = composicao.Select(i => i.ItemSocialId).OrderBy(id => id).ToArray();
        var itens = await _db.ItensSociais
            .FromSqlInterpolated($"SELECT * FROM \"ItensSociais\" WHERE \"Id\" = ANY({ids}) ORDER BY \"Id\" FOR UPDATE")
            .ToDictionaryAsync(i => i.Id, ct);

        var paraCalculo = composicao
            .Select(c => new ItemParaCalculo(
                itens[c.ItemSocialId].Nome, itens[c.ItemSocialId].Unidade, c.Quantidade, itens[c.ItemSocialId].EstoqueAtual))
            .ToList();

        var faltas = CapacidadeDaCesta.Faltas(paraCalculo, request.Quantidade);

        if (faltas.Count > 0)
            throw new ConflitoException(MensagemDeFalta(request.Quantidade, faltas, CapacidadeDaCesta.PodeMontar(paraCalculo)));

        var montagem = MontagemCesta.Criar(request.Quantidade, modelo.CestasProntas, _usuarioAtual.Id);
        _db.MontagensCesta.Add(montagem);

        foreach (var item in composicao)
        {
            var estoque = itens[item.ItemSocialId];
            var anterior = estoque.EstoqueAtual;

            estoque.Retirar(item.Quantidade * request.Quantidade);

            _db.MovimentacoesSociais.Add(MovimentacaoSocial.Montagem(
                estoque.Id, anterior, estoque.EstoqueAtual, montagem.Id, _usuarioAtual.Id));
        }

        modelo.AdicionarCestasProntas(request.Quantidade);

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new CestasMontadasResult(request.Quantidade, modelo.CestasProntas);
    }

    private static string MensagemDeFalta(int pedidas, IReadOnlyList<FaltaParaCesta> faltas, int podeMontar)
    {
        var itens = string.Join(", ", faltas.Select(f => $"{f.Nome} (tem {f.Tem}, precisa de {f.Precisa})"));
        var cestas = pedidas == 1 ? "1 cesta" : $"{pedidas} cestas";
        var limite = podeMontar == 0 ? "Não dá para montar nenhuma agora." : $"Dá para montar até {podeMontar}.";

        return $"Não dá para montar {cestas}. Falta: {itens}. {limite}";
    }
}
