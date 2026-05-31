using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentinelAuth.Domain.Shared;

namespace SentinelAuth.API.Controllers
{
    [ApiController]
    public abstract class BaseControllerAPI : ControllerBase
    {
        protected readonly ISender Sender;

        protected BaseControllerAPI(ISender sender)
        {
            Sender = sender;
        }
        
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Value is null ? NoContent() : Ok(result.Value);
            }

            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                statusCode: statusCode,
                title: result.Error.Code,
                detail: result.Error.Message
            );
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return NoContent();
            }

            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                statusCode: statusCode,
                title: result.Error.Code,
                detail: result.Error.Message
            );
        }
    }
}
