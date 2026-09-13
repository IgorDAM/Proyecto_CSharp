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

    public async Task<AmarreDto> AssignBarcoAsync(long id, AsignarBarcoDto dto, CancellationToken ct = default)
    {
        var amarre = await _amarreRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Amarre), id);

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
