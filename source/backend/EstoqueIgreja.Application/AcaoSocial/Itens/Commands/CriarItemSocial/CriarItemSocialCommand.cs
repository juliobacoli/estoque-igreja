using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;

public record CriarItemSocialCommand(string Nome, string Unidade) : IRequest<ItemSocialCriadoResult>;

public record ItemSocialCriadoResult(Guid Id, string Nome, string Unidade, int EstoqueAtual);
