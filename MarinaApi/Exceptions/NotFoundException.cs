namespace MarinaApi.Exceptions;

/// <summary>
/// Mejora sobre el proyecto Java: allí, "no encontrado" se resolvía devolviendo
/// null y comprobando "if (x != null)" en cada controlador (Cap. 13). Aquí se
/// lanza una excepción de dominio específica, que el middleware global
/// (ver Middleware/ExceptionHandlingMiddleware.cs) convierte automáticamente
/// en una respuesta 404 uniforme, sin repetir el chequeo en cada endpoint.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object id)
        : base($"{entidad} con Id {id} no encontrado.") { }
}
