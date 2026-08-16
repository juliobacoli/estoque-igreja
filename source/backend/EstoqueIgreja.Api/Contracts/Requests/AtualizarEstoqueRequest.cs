using System.ComponentModel.DataAnnotations;

namespace EstoqueIgreja.Api.Contracts.Requests;

public record AtualizarEstoqueRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero.")]
    public required int NovaQuantidade { get; init; }
}
