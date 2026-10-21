using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MarinaApi.Repositories;

public static class PaginacionExtensions
{
    /// <summary>
    /// Pagina una consulta de entidades. Es el caso común: la proyección es la identidad.
    /// </summary>
    public static async Task<(List<T> Elementos, int Total)> PaginarAsync<T>(
        this IQueryable<T> consulta,
        Expression<Func<T, long>> ordenarPor,
        int pagina,
        int tamanio,
        CancellationToken ct = default) =>
        await consulta.PaginarAsync(ordenarPor, x => x, pagina, tamanio, ct);

    /// <summary>
    /// Devuelve la página pedida y el total de filas de la consulta, proyectando cada
    /// elemento a <typeparamref name="TResultado"/> (p. ej. un DTO) DESPUÉS de ordenar y
    /// recortar. El orden se aplica sobre la entidad (<typeparamref name="T"/>) porque EF
    /// Core no puede ordenar por una propiedad de un objeto creado con "new Dto(...)".
    /// <paramref name="pagina"/> empieza en 1; Skip cuenta filas desde 0. Hace falta
    /// <paramref name="ordenarPor"/>: sin ORDER BY, SQL no garantiza el orden.
    /// </summary>
    public static async Task<(List<TResultado> Elementos, int Total)> PaginarAsync<T, TResultado>(
        this IQueryable<T> consulta,
        Expression<Func<T, long>> ordenarPor,
        Expression<Func<T, TResultado>> proyeccion,
        int pagina,
        int tamanio,
        CancellationToken ct = default)
    {
        var total = await consulta.CountAsync(ct);

        var elementos = await consulta
            .OrderBy(ordenarPor)
            .Skip((pagina - 1) * tamanio)
            .Take(tamanio)
            .Select(proyeccion)
            .ToListAsync(ct);

        return (elementos, total);
    }
}
