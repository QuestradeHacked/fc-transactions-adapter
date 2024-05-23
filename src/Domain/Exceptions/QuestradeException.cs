namespace Domain.Exceptions;

public class QuestradeException : Exception
{
    public QuestradeException(string message) : base(message)
    {
    }

    public QuestradeException(string message, Exception ex) : base(message, ex)
    {
    }
}
