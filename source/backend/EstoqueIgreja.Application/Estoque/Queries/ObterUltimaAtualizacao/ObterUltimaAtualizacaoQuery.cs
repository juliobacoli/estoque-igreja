using MediatR;

namespace EstoqueIgreja.Application.Estoque.Queries.ObterUltimaAtualizacao;

public record ObterUltimaAtualizacaoQuery : IRequest<UltimaAtualizacaoResult>;

/// <summary>
/// <c>Data</c> é null quando o estoque nunca foi contado.
/// </summary>
public record UltimaAtualizacaoResult(DateTime? Data);
