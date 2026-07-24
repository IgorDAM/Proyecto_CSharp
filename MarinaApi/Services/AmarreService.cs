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
}

public class AmarreService : IAmarreService
{
    private readonly IAmarreRepository _amarreRepository;

    public AmarreService(IAmarreRepository amarreRepository)
    {
        _amarreRepository = amarreRepository;
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
}
