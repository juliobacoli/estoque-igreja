using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Queries.ListarItensSociais;

public record ListarItensSociaisQuery : IRequest<IReadOnlyList<ItemSocialListado>>;

public record ItemSocialListado(Guid Id, string Nome, string Unidade, int EstoqueAtual);
