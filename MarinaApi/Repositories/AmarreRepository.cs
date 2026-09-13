using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Models;

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

    public async Task<Amarre?> FindByUbicacionAsync(string ubicacion, CancellationToken ct = default) =>
        await _context.Amarres.FirstOrDefaultAsync(a => a.Ubicacion == ubicacion, ct);

    public async Task<List<Amarre>> FindByPrecioLessThanAsync(double precio, CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.Precio < precio).ToListAsync(ct);

    public async Task<List<Amarre>> FindByElectricidadAsync(bool electricidad, CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.Electricidad == electricidad).ToListAsync(ct);

    public async Task<List<Amarre>> FindLibresAsync(CancellationToken ct = default) =>
        await _context.Amarres.Where(a => a.BarcoId == null).ToListAsync(ct);
    public async Task<Amarre?> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default) =>
    await _context.Amarres.FirstOrDefaultAsync(a => a.BarcoId == barcoId, ct);
}
