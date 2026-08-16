namespace EstoqueIgreja.Application.Common.Interfaces;

/// <summary>
/// Identifica o usuário autenticado na requisição atual. Implementado na camada Api,
/// que é quem enxerga o HttpContext e as claims do cookie.
/// </summary>
public interface IUsuarioAtual
{
    Guid Id { get; }
}
