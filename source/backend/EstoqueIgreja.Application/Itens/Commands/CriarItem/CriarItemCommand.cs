using MediatR;

namespace EstoqueIgreja.Application.Itens.Commands.CriarItem;

public record CriarItemCommand(string Nome, string Unidade) : IRequest<ItemCriadoResult>;

public record ItemCriadoResult(Guid Id, string Nome, string Unidade, int EstoqueAtual);
