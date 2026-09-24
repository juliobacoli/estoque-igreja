using EstoqueIgreja.Domain.Enums;

namespace EstoqueIgreja.Api.Services;

/// <summary>
/// Cada módulo vira um claim no cookie e uma policy de mesmo nome. Quem não tem
/// o módulo recebe 403 em todos os endpoints dele.
/// </summary>
public static class Politicas
{
    public const string ClaimModulo = "modulo";

    public const string Obreiros = nameof(Modulo.Obreiros);
    public const string AcaoSocial = nameof(Modulo.AcaoSocial);
}
