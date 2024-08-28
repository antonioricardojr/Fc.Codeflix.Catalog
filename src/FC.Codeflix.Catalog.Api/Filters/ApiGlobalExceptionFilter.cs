using FC.Codeflix.Catalog.Application.Exceptions;
using FC.Codeflix.Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FC.Codeflix.Catalog.Api.Filters;

public class ApiGlobalExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _env;
    
    public ApiGlobalExceptionFilter(IHostEnvironment env)
    {
        _env = env;
    }
    
    public void OnException(ExceptionContext context)
    {
        var details = new ProblemDetails();
        var exception = context.Exception;

        if (_env.IsDevelopment())
        {
            details.Extensions.Add("Stacktrace", exception.StackTrace);
        }

        if (exception is EntityValidationException)
        {
            details.Title = "One or more validation errors occurred";
            details.Status = (int) StatusCodes.Status422UnprocessableEntity;
            details.Detail = exception.Message;
            details.Type = "UnprocessableEntity";
        } else if (exception is NotFoundException)
        {
            details.Title = "Not Found";
            details.Status = (int) StatusCodes.Status404NotFound;
            details.Detail = exception.Message;
            details.Type = "NotFound";
        } else if (exception is RelatedAggregateException)
        {
            details.Title = "Invalid Related Aggregate";
            details.Status = (int) StatusCodes.Status422UnprocessableEntity;
            details.Detail = exception.Message;
            details.Type = "RelatedAggregate";
        }
        else
        {
            details.Title = "An unexpected error occurred";
            details.Status = (int) StatusCodes.Status422UnprocessableEntity;
            details.Type = "UnexpectedError";
            details.Detail = exception.Message;
        }

        context.HttpContext.Response.StatusCode = (int) details.Status;
        context.Result = new ObjectResult(details);
        context.ExceptionHandled = true;
    }
}