using MarinaApi.Data;
using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Mapping;
using MarinaApi.Repositories;

namespace MarinaApi.Services;

/// <summary>Equivalente a BarcoService.java (Capítulo 12).</summary>
public interface IBarcoService
{
    Task<List<BarcoDto>> FindAllAsync(CancellationToken ct = default);
    Task<BarcoDto> FindByIdAsync(long id, CancellationToken ct = default);
    Task<BarcoDto> CreateAsync(BarcoRequestDto dto, CancellationToken ct = default);
    Task<BarcoDto> UpdateAsync(long id, BarcoRequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<BarcoDto>> FindByTipoAsync(string tipo, CancellationToken ct = default);
    Task<long> CountByTipoAsync(string tipo, CancellationToken ct = default);
    Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default);

    Task<List<BarcoDto>> FindByCapacidadMinimaAsync(int capacidadMinima, CancellationToken ct = default);
}

public class BarcoService : IBarcoService
{
    private readonly IBarcoRepository _barcoRepository;
    private readonly MarinaDbContext? _context;
    private readonly ILogger<BarcoService> _logger;

    // Constructor principal para producción (con MarinaDbContext)
    public BarcoService(IBarcoRepository barcoRepository, MarinaDbContext context, ILogger<BarcoService> logger)
    {
        _barcoRepository = barcoRepository;
        _context = context;
        _logger = logger;
    }

    // Constructor para tests (sin MarinaDbContext)
    public BarcoService(IBarcoRepository barcoRepository, ILogger<BarcoService> logger)
        : this(barcoRepository, null!, logger) { }

    public async Task<List<BarcoDto>> FindAllAsync(CancellationToken ct = default)
    {
        var barcos = await _barcoRepository.FindAllAsync(ct);
        return barcos.Select(b => b.ToDto()).ToList();
    }

    public async Task<BarcoDto> FindByIdAsync(long id, CancellationToken ct = default)
    {
        var barco = await _barcoRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Barco), id);
        return barco.ToDto();
    }

    public async Task<BarcoDto> CreateAsync(BarcoRequestDto dto, CancellationToken ct = default)
    {
        var barco = dto.ToEntity();
        var creado = await _barcoRepository.AddAsync(barco, ct);
        _logger.LogInformation("Barco creado: {Id} - {Nombre}", creado.Id, creado.Nombre);
        return creado.ToDto();
    }

    public async Task<BarcoDto> UpdateAsync(long id, BarcoRequestDto dto, CancellationToken ct = default)
    {
        var barco = await _barcoRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Barco), id);

        // CHECKPOINT 5.4: Inspecciona Change Tracking ANTES (solo si el context está disponible)
        if (_context != null)
        {
            var stateAntes = _context.Entry(barco).State;
            _logger.LogInformation("🔍 ANTES de UpdateFromDto: EntityState = {State}", stateAntes);

            barco.UpdateFromDto(dto);

            var stateDespues = _context.Entry(barco).State;
            _logger.LogInformation("🔍 DESPUÉS de UpdateFromDto: EntityState = {State}", stateDespues);
            _logger.LogInformation("✅ EF Core detectó cambios automáticamente (Change Tracking)");
        }
        else
        {
            barco.UpdateFromDto(dto);
        }

        await _barcoRepository.UpdateAsync(barco, ct);
        return barco.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var barco = await _barcoRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Barco), id);
        await _barcoRepository.DeleteAsync(barco, ct);
    }

    public async Task<List<BarcoDto>> FindByTipoAsync(string tipo, CancellationToken ct = default)
    {
        var barcos = await _barcoRepository.FindByTipoAsync(tipo, ct);
        return barcos.Select(b => b.ToDto()).ToList();
    }

    public Task<long> CountByTipoAsync(string tipo, CancellationToken ct = default) =>
        _barcoRepository.CountByTipoAsync(tipo, ct);

    public async Task<double> GetPromedioEsloraByTipoAsync(string tipo, CancellationToken ct = default)
    {
        var promedio = await _barcoRepository.GetPromedioEsloraByTipoAsync(tipo, ct);
        _logger.LogInformation("📊 Promedio de eslora para tipo '{Tipo}': {Promedio:F2} metros", tipo, promedio);
        return promedio;
    }

    public async Task<List<BarcoDto>> FindByCapacidadMinimaAsync(int capacidadMinima, CancellationToken ct = default)
    {
        var barcos = await _barcoRepository.FindByCapacidadGreaterThanOrEqualAsync(capacidadMinima, ct);
        return barcos.Select(b => b.ToDto()).ToList();
    }
}
