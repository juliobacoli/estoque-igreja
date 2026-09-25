namespace EstoqueIgreja.Api.Contracts.Requests;

public record RegistrarEntradaRequest
{
    public required int Quantidade { get; init; }
}
