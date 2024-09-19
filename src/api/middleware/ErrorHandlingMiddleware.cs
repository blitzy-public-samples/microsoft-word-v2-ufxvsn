using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MicrosoftWord.Core.Exceptions;

namespace MicrosoftWord.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Try to execute the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // If an exception is caught, call HandleExceptionAsync
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception
            _logger.LogError(exception, "An unhandled exception has occurred.");

            // Determine the HTTP status code based on the exception type
            var statusCode = GetStatusCode(exception);

            // Create an error response object
            var response = new
            {
                error = new
                {
                    message = exception.Message,
                    exceptionType = exception.GetType().Name
                }
            };

            // Serialize the error response to JSON
            var payload = JsonConvert.SerializeObject(response);

            // Set the response status code and content type
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Write the error response to the response body
            await context.Response.WriteAsync(payload);
        }

        private int GetStatusCode(Exception exception)
        {
            // Switch on the exception type to determine the appropriate HTTP status code
            return exception switch
            {
                NotFoundException _ => (int)HttpStatusCode.NotFound,
                UnauthorizedException _ => (int)HttpStatusCode.Unauthorized,
                ForbiddenException _ => (int)HttpStatusCode.Forbidden,
                ValidationException _ => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder builder)
        {
            // Add the ErrorHandlingMiddleware to the application pipeline
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}