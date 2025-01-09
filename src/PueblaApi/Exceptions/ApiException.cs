namespace PueblaApi.Exceptions;
public class ApiException : Exception
{
    public ApiException() : base() { }
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception e) : base(message, e) { }
}