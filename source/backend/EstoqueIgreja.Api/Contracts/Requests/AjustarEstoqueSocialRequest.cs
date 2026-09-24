namespace EstoqueIgreja.Api.Contracts.Requests;

public record AjustarEstoqueSocialRequest
{
    public required int NovaQuantidade { get; init; }
    public string Motivo { get; init; } = "";
}
