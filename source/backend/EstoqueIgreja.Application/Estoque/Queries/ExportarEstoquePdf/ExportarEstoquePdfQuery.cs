using MediatR;

namespace EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;

public record ExportarEstoquePdfQuery : IRequest<byte[]>;

public record ItemDoRelatorio(string Nome, string Unidade, int EstoqueAtual);
