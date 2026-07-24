using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Models;

namespace MarinaApi.Repositories;

/// <summary>
/// Equivalente a BarcoRepository.java (Capítulo 11), incluyendo las consultas
/// derivadas (findByTipo, findByNombre...) y la consulta con JOIN FETCH.
/// </summary>
public interface IBarcoRepository : IGenericRepository<Barco>
{
    Task<Barco?> FindByNombreAsync(string nombre, CancellationToken ct = default);
    Task<List<Barco>> FindByTipoAsync(string tipo, CancellationToken ct = default);
    Task<List<Barco>> FindByEsloraGreaterThanAsync(int eslora, CancellationToken ct = default);
    Task<List<Barco>> FindByCapacidadGreaterThanAsync(int capacidad, CancellationToken ct = default);
    Task<long> CountByTipoAsync(string tipo, CancellationToken ct = default);

    // Equivalente a: @Query("SELECT b FROM Barco b JOIN FETCH b.regatas WHERE b.id = :id")
    Task<Barco?> FindByIdWithRegatasAsync(long id, CancellationToken ct = default);

    // Equivalente a: @Query("SELECT b FROM Barco b WHERE b.amarre IS NULL")
    Task<List<Barco>> FindSinAmarreAsync(CancellationToken ct = default);

    // Equivalente a: @Query("SELECT AVG(b.eslora) FROM Barco b WHERE b.tipo = :tipo") (Cap. 8.2.5)
    Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default);
}

public class BarcoRepository : GenericRepository<Barco>, IBarcoRepository
{
    private readonly MarinaDbContext _context;

    public BarcoRepository(MarinaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Barco?> FindByNombreAsync(string nombre, CancellationToken ct = default) =>
        await _context.Barcos.FirstOrDefaultAsync(b => b.Nombre == nombre, ct);

    public async Task<List<Barco>> FindByTipoAsync(string tipo, CancellationToken ct = default) =>
        await _context.Barcos.Where(b => b.Tipo == tipo).ToListAsync(ct);

    public async Task<List<Barco>> FindByEsloraGreaterThanAsync(int eslora, CancellationToken ct = default) =>
        await _context.Barcos.Where(b => b.Eslora > eslora).ToListAsync(ct);

    public async Task<List<Barco>> FindByCapacidadGreaterThanAsync(int capacidad, CancellationToken ct = default) =>
        await _context.Barcos.Where(b => b.Capacidad > capacidad).ToListAsync(ct);

    public async Task<long> CountByTipoAsync(string tipo, CancellationToken ct = default) =>
        await _context.Barcos.LongCountAsync(b => b.Tipo == tipo, ct);

    // Include() es el equivalente exacto de JOIN FETCH: carga la relación
    // en la MISMA consulta SQL, evitando el problema N+1.
    public async Task<Barco?> FindByIdWithRegatasAsync(long id, CancellationToken ct = default) =>
        await _context.Barcos
            .Include(b => b.Regatas)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<List<Barco>> FindSinAmarreAsync(CancellationToken ct = default) =>
        await _context.Barcos.Where(b => b.Amarre == null).ToListAsync(ct);

    public async Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default) =>
        await _context.Barcos
            .Where(b => b.Tipo == tipo)
            .AverageAsync(b => (double)b.Eslora, ct);
}
