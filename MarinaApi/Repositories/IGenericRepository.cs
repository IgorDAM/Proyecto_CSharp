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
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<bool> ExistsAsync(long id, CancellationToken ct = default);
}
