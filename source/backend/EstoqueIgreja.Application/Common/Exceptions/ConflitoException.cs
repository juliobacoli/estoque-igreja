namespace EstoqueIgreja.Application.Common.Exceptions;

/// <summary>
/// Regra de negócio violada — vira HTTP 400 com a mensagem no corpo.
/// </summary>
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem)
    {
    }
}
