using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Models;
using MarinaApi.Dtos;
using MarinaApi.Mapping;

namespace MarinaApi.Repositories;

/// <summary>Equivalente a RegataRepository.java.</summary>
public interface IRegataRepository : IGenericRepository<Regata>
{
    Task<Regata?> FindByNombreAsync(string nombre, CancellationToken ct = default);
    Task<List<Regata>> FindByLugarAsync(string lugar, CancellationToken ct = default);
    Task<List<Regata>> FindByDistanciaGreaterThanAsync(int distancia, CancellationToken ct = default);
    Task<Regata?> FindByIdWithBarcosAsync(long id, CancellationToken ct = default);
    Task<List<RegataDto>> FindAllConContadorAsync(CancellationToken ct = default);
    Task<List<RegataDto>> FindByLugarConContadorAsync(string lugar, CancellationToken ct = default);
}

public class RegataRepository : GenericRepository<Regata>, IRegataRepository
{
    private readonly MarinaDbContext _context;

    public RegataRepository(MarinaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Regata?> FindByNombreAsync(string nombre, CancellationToken ct = default) =>
        await _context.Regatas.FirstOrDefaultAsync(r => r.Nombre == nombre, ct);

    public async Task<List<Regata>> FindByLugarAsync(string lugar, CancellationToken ct = default) =>
        await _context.Regatas.Where(r => r.Lugar == lugar).ToListAsync(ct);

    public async Task<List<Regata>> FindByDistanciaGreaterThanAsync(int distancia, CancellationToken ct = default) =>
        await _context.Regatas.Where(r => r.Distancia > distancia).ToListAsync(ct);

    public async Task<Regata?> FindByIdWithBarcosAsync(long id, CancellationToken ct = default) =>
        await _context.Regatas.Include(r => r.Barcos).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<RegataDto>> FindAllConContadorAsync(CancellationToken ct = default) =>
await _context.Regatas
    .Select(RegataMapper.ToDtoProjection)
    .ToListAsync(ct);

    public async Task<List<RegataDto>> FindByLugarConContadorAsync(string lugar, CancellationToken ct = default) =>
        await _context.Regatas
            .Where(r => r.Lugar == lugar)
            .Select(RegataMapper.ToDtoProjection)
            .ToListAsync(ct);
}
