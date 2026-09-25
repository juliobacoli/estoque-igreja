namespace EstoqueIgreja.UnitTests.Helpers;

public static class Reflexao
{
    /// <summary>
    /// As entidades têm setters privados. Nos testes que dependem de valores fixos
    /// (datas, ids), o valor é definido por reflexão.
    /// </summary>
    public static void Definir(object alvo, string propriedade, object valor)
        => alvo.GetType().GetProperty(propriedade)!.SetValue(alvo, valor);

    /// <summary>
    /// Lê uma propriedade de objeto anônimo devolvido por controller ou middleware.
    /// </summary>
    public static object? Ler(object alvo, string propriedade)
        => alvo.GetType().GetProperty(propriedade)!.GetValue(alvo);
}
