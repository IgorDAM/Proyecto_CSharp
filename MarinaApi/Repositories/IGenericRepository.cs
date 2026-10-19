using System.Linq.Expressions;

namespace MarinaApi.Repositories;

/// <summary>
/// Repositorio genérico. Mejora directa sobre el proyecto Java: allí, BarcoDAO,
/// AmarreDAO y RegataDAO eran interfaces casi idénticas (Cap. 7), y sus
/// implementaciones repetían literalmente el mismo código con try-with-resources
/// + beginTransaction + commit para cada entidad (~150 líneas cada una).
/// Aquí, una única implementación genérica sirve para las tres, totalmente async.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> FindByIdAsync(long id, CancellationToken ct = default);
    Task<List<T>> FindAllAsync(CancellationToken ct = default);
    /// <summary>
    /// Devuelve una página de entidades y el total de filas de la tabla.
    /// <paramref name="pagina"/> empieza en 1 (como ?pagina=1 en la URL).
    /// Hace falta <paramref name="ordenarPor"/>: sin un ORDER BY, SQL no
    /// garantiza el orden de las filas, y dos peticiones seguidas podrían
    /// repetir u omitir elementos entre páginas.
    /// </summary>
    Task<(List<T> Elementos, int Total)> FindPageAsync(
        Expression<Func<T, long>> ordenarPor,
        int pagina,
        int tamanio,
        CancellationToken ct = default);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<bool> ExistsAsync(long id, CancellationToken ct = default);
}
