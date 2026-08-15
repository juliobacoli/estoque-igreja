namespace EstoqueIgreja.Application.Common.Exceptions;

public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem)
    {
    }
}
