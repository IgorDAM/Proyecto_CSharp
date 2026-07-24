using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;

namespace MarinaApi.Repositories;

/// <summary>
/// Única implementación para cualquier entidad. Equivale, todas juntas, a
/// BarcoDAOImpl + AmarreDAOImpl + RegataDAOImpl del Capítulo 7 de Java —
/// pero sin duplicación, sin gestión manual de Session/Transaction (EF Core
/// la gestiona internamente en el DbContext), y con soporte de cancelación
/// (CancellationToken) para poder abortar peticiones HTTP en curso, algo que
/// el proyecto Java no contemplaba.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly MarinaDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(MarinaDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Equivalente a session.find(T.class, id) — sin transacción, es solo lectura.
    public async Task<T?> FindByIdAsync(long id, CancellationToken ct = default) =>
        await _dbSet.FindAsync(new object[] { id }, ct);

    // Equivalente a session.createQuery("from T", T.class).getResultList()
    public async Task<List<T>> FindAllAsync(CancellationToken ct = default) =>
        await _dbSet.ToListAsync(ct);

    // Equivalente a las consultas HQL personalizadas del Capítulo 8, pero
    // expresadas como lambda LINQ en vez de strings HQL con nombres de atributo.
    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _dbSet.Where(predicate).ToListAsync(ct);

    // Equivalente a beginTransaction() → session.save() → commit(), pero
    // EF Core no confirma hasta SaveChangesAsync() — normalmente se llama
    // una sola vez desde el servicio, agrupando varias operaciones en UNA
    // transacción real, algo que Java no hacía (cada save() era su propia
    // transacción aislada).
    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken ct = default) =>
        await FindByIdAsync(id, ct) is not null;
}
