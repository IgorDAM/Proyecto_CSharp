using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Mapping;
using MarinaApi.Repositories;

namespace MarinaApi.Services;

/// <summary>Equivalente a RegataService.java, incluida la gestión N:M con Barco.</summary>
public interface IRegataService
{
    Task<List<RegataDto>> FindAllAsync(CancellationToken ct = default);
    Task<RegataDto> FindByIdAsync(long id, CancellationToken ct = default);
    Task<RegataDto> CreateAsync(RegataRequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<RegataDto>> FindByLugarAsync(string lugar, CancellationToken ct = default);
    Task InscribirBarcoAsync(long regataId, long barcoId, CancellationToken ct = default);
    Task DesinscribirBarcoAsync(long regataId, long barcoId, CancellationToken ct = default);
}

public class RegataService : IRegataService
{
    private readonly IRegataRepository _regataRepository;
    private readonly IBarcoRepository _barcoRepository;
    private readonly MarinaDbContext _context;

    public RegataService(IRegataRepository regataRepository, IBarcoRepository barcoRepository, MarinaDbContext context)
    {
        _regataRepository = regataRepository;
        _barcoRepository = barcoRepository;
        _context = context;
    }

    public async Task<List<RegataDto>> FindAllAsync(CancellationToken ct = default) =>
        (await _regataRepository.FindAllAsync(ct)).Select(r => r.ToDto()).ToList();

    public async Task<RegataDto> FindByIdAsync(long id, CancellationToken ct = default)
    {
        var regata = await _regataRepository.FindByIdWithBarcosAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Regata), id);
        return regata.ToDto();
    }

    public async Task<RegataDto> CreateAsync(RegataRequestDto dto, CancellationToken ct = default)
    {
        var creada = await _regataRepository.AddAsync(dto.ToEntity(), ct);
        return creada.ToDto();
    }

    public async Task<List<RegataDto>> FindByLugarAsync(string lugar, CancellationToken ct = default) =>
        (await _regataRepository.FindByLugarAsync(lugar, ct)).Select(r => r.ToDto()).ToList();

    /// <summary>
    /// Inscribe un barco en una regata (relación N:M). Equivalente a
    /// RegataService.inscribirBarco() de Java. Barco es el lado "owning" de la
    /// relación (ver MarinaDbContext), así que se añade la regata a
    /// barco.Regatas, igual que en Java se modificaba barco.getRegatas().
    /// </summary>
    public async Task InscribirBarcoAsync(long regataId, long barcoId, CancellationToken ct = default)
    {
        var regata = await _regataRepository.FindByIdAsync(regataId, ct)
            ?? throw new NotFoundException(nameof(Models.Regata), regataId);
        var barco = await _barcoRepository.FindByIdWithRegatasAsync(barcoId, ct)
            ?? throw new NotFoundException(nameof(Models.Barco), barcoId);

        if (barco.Regatas.All(r => r.Id != regataId))
        {
            barco.Regatas.Add(regata);
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <summary>Equivalente a RegataService.desinscribirBarco() de Java.</summary>
    public async Task DesinscribirBarcoAsync(long regataId, long barcoId, CancellationToken ct = default)
    {
        var barco = await _barcoRepository.FindByIdWithRegatasAsync(barcoId, ct)
            ?? throw new NotFoundException(nameof(Models.Barco), barcoId);

        barco.Regatas.RemoveAll(r => r.Id == regataId);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Elimina una regata. Mejora respecto a Java: allí "deleteWithCleanup"
    /// desvinculaba manualmente la regata de cada barco antes de borrarla, con
    /// un bucle "for" y un save() por cada barco (varias transacciones
    /// implícitas). Aquí, una única transacción explícita de EF Core envuelve
    /// toda la operación: o se desvincula + borra todo, o no se aplica nada,
    /// que es justo la garantía de atomicidad que el Capítulo 7 de Java pedía
    /// pero no llegaba a implementar con rollback real.
    /// </summary>
    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var regata = await _regataRepository.FindByIdWithBarcosAsync(id, ct)
                ?? throw new NotFoundException(nameof(Models.Regata), id);

            foreach (var barco in regata.Barcos.ToList())
            {
                barco.Regatas.Remove(regata);
            }

            _context.Regatas.Remove(regata);
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
