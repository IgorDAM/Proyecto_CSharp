using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Models;
using MySqlConnector;
using MarinaApi.Exceptions;

namespace MarinaApi.Repositories;

/// <summary>Equivalente a AmarreRepository.java.</summary>
public interface IAmarreRepository : IGenericRepository<Amarre>
{
    Task<Amarre?> FindByUbicacionAsync(string ubicacion, CancellationToken ct = default);
    Task<List<Amarre>> FindByPrecioLessThanAsync(double precio, CancellationToken ct = default);
    Task<List<Amarre>> FindByElectricidadAsync(bool electricidad, CancellationToken ct = default);
    Task<List<Amarre>> FindLibresAsync(CancellationToken ct = default); // findByBarcoIsNull en Java
    Task<Amarre?> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);
}

public class AmarreRepository : GenericRepository<Amarre>, IAmarreRepository
{
    private readonly MarinaDbContext _context;

    public AmarreRepository(MarinaDbContext context) : base(context)
    {
        _context = context;
    }
    /// <summary>
    /// Guarda los cambios de un Amarre. Si la base de datos rechaza el guardado porque
    /// el Barco ya tiene otro Amarre (error 1062 de MySQL, "Duplicate entry", por el
    /// índice único de <c>Amarres.BarcoId</c>), lo traduce a <see cref="ConflictException"/>
    /// para que la API responda 409 en vez de 500 (#155). Cualquier otro error de
    /// base de datos se deja pasar tal cual.
    /// </summary>
    public override async Task UpdateAsync(Amarre entity, CancellationToken ct = default)
    {
        try
        {
            await base.UpdateAsync(entity, ct);
        }
        // Solo el 1062: es lo único que sabemos explicar al cliente. Un deadlock o una
        // caída de red deben seguir siendo 500. En Amarres no hay más índices únicos,
        // así que un 1062 aquí siempre significa "ese barco ya tiene amarre".
        catch (DbUpdateException ex) when (ex.InnerException is MySqlException { Number: 1062 })
        {
            throw new ConflictException(
                $"El Barco con Id {entity.BarcoId} ya tiene asignado otro Amarre.");
        }
    }
    public async Task<Amarre?> FindByUbicacionAsync(string ubicacion, CancellationToken ct = default) =>
        await _context.Amarres.FirstOrDefaultAsync(a => a.Ubicacion == ubicacion, ct);

    public async Task<List<Amarre>> FindByPrecioLessThanAsync(double precio, CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.Precio < precio).OrderBy(a => a.Id).ToListAsync(ct);

    public async Task<List<Amarre>> FindByElectricidadAsync(bool electricidad, CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.Electricidad == electricidad).OrderBy(a => a.Id).ToListAsync(ct);

    public async Task<List<Amarre>> FindLibresAsync(CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.BarcoId == null).OrderBy(a => a.Id).ToListAsync(ct);
    public async Task<Amarre?> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default) =>
    await _context.Amarres.FirstOrDefaultAsync(a => a.BarcoId == barcoId, ct);
}
