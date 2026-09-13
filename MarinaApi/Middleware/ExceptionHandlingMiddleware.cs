using System.Net;
using System.Text.Json;
using MarinaApi.Exceptions;

namespace MarinaApi.Middleware;

/// <summary>
/// Manejo de errores centralizado — otra mejora sobre el proyecto Java, que
/// no tenía manejo global de errores: cada controlador comprobaba null "a mano"
/// y devolvía 404/500 de forma inconsistente. Aquí, un único middleware
/// intercepta cualquier excepción de toda la API y produce una respuesta
/// uniforme en formato "problem+json" (RFC 7807), el estándar de ASP.NET Core.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError,
                "Ha ocurrido un error inesperado.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, HttpStatusCode status, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title = status.ToString(),
            status = (int)status,
            detail
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
