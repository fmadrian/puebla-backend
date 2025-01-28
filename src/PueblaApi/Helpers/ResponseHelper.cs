using PueblaApi.DTOS.Base;

namespace PueblaApi.Helpers;

public class ResponseHelper
{
    #region Successful responses
    public static Response<T> SuccessfulResponse<T>(T arg)
    {
        return new Response<T>()
        {
            Result = true,
            Object = arg
        };
    }
    public static Response<object> SuccessfulResponse(string msg = "")
    {
        return new Response<object>()
        {
            Result = true,
            Message = msg
        };
    }
    public static Response<object> SuccessfulResponse()
    {
        return new Response<object>()
        {
            Result = true
        };
    }
    #endregion

    #region Unsuccessful responses
    public static Response<T> UnsuccessfulResponse<T>(List<string> errors = null)
    {
        return new Response<T>()
        {
            Result = false,
            Errors = errors
        };
    }
    public static Response<string> UnsuccessfulResponse(List<string> errors = null, string msg = "")
    {
        return new Response<string>()
        {
            Result = false,
            Errors = errors,
            Message = msg
        };
    }
    public static Response<string> UnsuccessfulResponse(string msg = "")
    {
        return new Response<string>()
        {
            Result = false,
            Message = msg
        };
    }
    #endregion
}