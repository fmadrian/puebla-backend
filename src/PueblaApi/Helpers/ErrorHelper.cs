using Microsoft.AspNetCore.Mvc;
using PueblaApi.DTOS.Base;
namespace PueblaApi.RequestHelpers;
/**
    Helper methods that help to handle different exceptions that might arise in the API.
**/
public class ErrorHelper
{
    public static ActionResult Internal(ILogger logger, string message)
    {
        logger.LogError(message);
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = new List<string>(){
                "Internal server error."
            }
        });
        error.StatusCode = StatusCodes.Status500InternalServerError; // HTTP 500
        return error;
    }
    public static ActionResult Internal(ILogger logger, Exception e)
    {
        logger.LogError(e.Message);
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = new List<string>(){
                    "Internal server error."
                }
        });
        error.StatusCode = StatusCodes.Status500InternalServerError; // HTTP 500
        return error;
    }
    public static ActionResult Internal(ILogger logger, string message, List<string> errors)
    {
        logger.LogError(message);
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = errors
        });
        error.StatusCode = StatusCodes.Status500InternalServerError; // HTTP 500
        return error;
    }
    public static ActionResult Internal(ILogger logger, string message, string errorMessage)
    {
        logger.LogError(message);
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = new List<string>(){
                    errorMessage
            }
        });
        error.StatusCode = StatusCodes.Status500InternalServerError; // HTTP 500
        return error;
    }
    public static ActionResult BadRequest(ILogger logger, string message)
    {
        logger.LogError(message);
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = new List<string>(){
                    message
                }
        });
        error.StatusCode = StatusCodes.Status400BadRequest; // HTTP 400
        return error;
    }
    public static ActionResult BadRequest(ILogger logger, List<string> errors)
    {
        logger.LogError("More than one error.");
        errors.ForEach(e =>
        {
            logger.LogError(e);
        });
        ObjectResult error = new ObjectResult(new Response<Object>()
        {
            Result = false,
            Errors = errors
        });
        error.StatusCode = StatusCodes.Status400BadRequest; // HTTP 400
        return error;
    }
}
