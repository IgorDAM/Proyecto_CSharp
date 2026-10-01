using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Mapping;
using MarinaApi.Repositories;

namespace MarinaApi.Services;

/// <summary>Equivalente a AmarreService.java.</summary>
public interface IAmarreService
{
    Task<List<AmarreDto>> FindAllAsync(CancellationToken ct = default);
    Task<AmarreDto> FindByIdAsync(long id, CancellationToken ct = default);
    Task<AmarreDto> CreateAsync(AmarreRequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<AmarreDto>> FindLibresAsync(CancellationToken ct = default);
    Task<List<AmarreDto>> FindConElectricidadAsync(CancellationToken ct = default);

    Task<AmarreDto> AssignBarcoAsync(long id, AsignarBarcoDto dto, CancellationToken ct = default);
}

public class AmarreService : IAmarreService
{
    private readonly IAmarreRepository _amarreRepository;
    private readonly IBarcoRepository _barcoRepository;


    public AmarreService(IAmarreRepository amarreRepository, IBarcoRepository barcoRepository)
    {
        _amarreRepository = amarreRepository;
        _barcoRepository = barcoRepository;
    }


    public async Task<List<AmarreDto>> FindAllAsync(CancellationToken ct = default) =>
        (await _amarreRepository.FindAllAsync(ct)).Select(a => a.ToDto()).ToList();

    public async Task<AmarreDto> FindByIdAsync(long id, CancellationToken ct = default)
    {
        var amarre = await _amarreRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Amarre), id);
        return amarre.ToDto();
    }

    public async Task<AmarreDto> CreateAsync(AmarreRequestDto dto, CancellationToken ct = default)
    {
        var creado = await _amarreRepository.AddAsync(dto.ToEntity(), ct);
        return creado.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var amarre = await _amarreRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Amarre), id);
        await _amarreRepository.DeleteAsync(amarre, ct);
    }

    public async Task<List<AmarreDto>> FindLibresAsync(CancellationToken ct = default) =>
        (await _amarreRepository.FindLibresAsync(ct)).Select(a => a.ToDto()).ToList();

    public async Task<List<AmarreDto>> FindConElectricidadAsync(CancellationToken ct = default) =>
        (await _amarreRepository.FindByElectricidadAsync(true, ct)).Select(a => a.ToDto()).ToList();

    /// <summary>
    /// Asigna un Barco existente a un Amarre (ticket #151). Reglas, en este orden:
    /// <list type="number">
    /// <item>El Amarre tiene que existir; si no, 404.</item>
    /// <item>Si el Amarre ya tiene ESE mismo Barco, se devuelve sin tocar nada:
    /// la operación es idempotente (#172).</item>
    /// <item>Si el Amarre está ocupado por OTRO Barco, 409: no se desaloja a nadie (#172).</item>
    /// <item>El Barco tiene que existir; si no, 404.</item>
    /// <item>El Barco no puede tener ya otro Amarre; si no, 409.</item>
    /// </list>
    /// </summary>
    public async Task<AmarreDto> AssignBarcoAsync(long id, AsignarBarcoDto dto, CancellationToken ct = default)
    {
        var amarre = await _amarreRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Amarre), id);

        // #172: repetir la misma asignación no es un error (idempotente):
        // se devuelve el amarre tal cual, sin volver a guardar.
        if (amarre.BarcoId == dto.BarcoId)
            return amarre.ToDto();

        // #172: pero meter un barco en un amarre ocupado por OTRO sí lo es:
        // no se desaloja a nadie sin avisar.
        if (amarre.BarcoId is not null)
            throw new ConflictException(
                $"El Amarre con Id {id} ya está ocupado por el Barco {amarre.BarcoId}.");

        var barcoExiste = await _barcoRepository.ExistsAsync(dto.BarcoId, ct);
        if (!barcoExiste)
            throw new NotFoundException(nameof(Models.Barco), dto.BarcoId);

        var amarreExistente = await _amarreRepository.FindByBarcoIdAsync(dto.BarcoId, ct);
        if (amarreExistente is not null)
            throw new ConflictException(
                $"El Barco con Id {dto.BarcoId} ya tiene asignado el Amarre {amarreExistente.Id}.");

        amarre.BarcoId = dto.BarcoId;
        await _amarreRepository.UpdateAsync(amarre, ct);
        return amarre.ToDto();
    }
}
