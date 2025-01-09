
namespace PueblaApi.DTOS.Base;

/// <summary>
/// Response that contains a generic object.
/// </summary>
/// <typeparam name="T">Type of response object to be sent back to client.</typeparam>
public class Response<T>
{
    public string? Message { get; set; }
    public bool Result { get; set; } // Was the operation finished?
    public List<string> Errors { get; set; } // List of errors.
    public T Object { get; set; } // Object to be returned in the response.
}