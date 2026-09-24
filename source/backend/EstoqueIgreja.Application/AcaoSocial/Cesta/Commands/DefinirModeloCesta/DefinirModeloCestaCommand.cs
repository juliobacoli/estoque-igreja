using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.DefinirModeloCesta;

public record DefinirModeloCestaCommand(IReadOnlyList<ItemDoModelo> Itens) : IRequest<Unit>;

public record ItemDoModelo(Guid ItemId, int Quantidade);
