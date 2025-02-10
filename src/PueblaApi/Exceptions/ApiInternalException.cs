namespace PueblaApi.Exceptions;
/// <summary>
/// Exception class used to represent server errors.
/// </summary>
public class ApiInternalException : Exception
{
    public ApiInternalException() : base() { }
    public ApiInternalException(string message) : base(message) { }
    public ApiInternalException(string message, Exception e) : base(message, e) { }
}