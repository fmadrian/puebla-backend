namespace PueblaApi.Exceptions;
public class EmailException : Exception
{
    public EmailException() : base() { }
    public EmailException(string message) : base(message) { }
    public EmailException(string message, Exception e) : base(message, e) { }
}