using MarinaApi.Dtos;
using MarinaApi.Exceptions;
using MarinaApi.Mapping;
using MarinaApi.Repositories;

namespace MarinaApi.Services;

public interface ITripulanteService
{
    Task<List<TripulanteDto>> FindAllAsync(CancellationToken ct = default);
    Task<TripulanteDto> FindByIdAsync(long id, CancellationToken ct = default);
    Task<TripulanteDto> CreateAsync(TripulanteRequestDto dto, CancellationToken ct = default);
    Task<TripulanteDto> UpdateAsync(long id, TripulanteRequestDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<TripulanteDto>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);
}


public class TripulanteService : ITripulanteService
{
    private readonly ITripulanteRepository _tripulanteRepository;
    private readonly IBarcoRepository _barcoRepository;

    public TripulanteService(ITripulanteRepository tripulanteRepository, IBarcoRepository barcoRepository)
    {
        _tripulanteRepository = tripulanteRepository;
        _barcoRepository = barcoRepository;
    }

    public async Task<List<TripulanteDto>> FindAllAsync(CancellationToken ct = default) =>
        (await _tripulanteRepository.FindAllAsync(ct)).Select(t => t.ToDto()).ToList();

    public async Task<TripulanteDto> FindByIdAsync(long id, CancellationToken ct = default)
    {
        var tripulante = await _tripulanteRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Tripulante), id);
        return tripulante.ToDto();
    }

    public async Task<TripulanteDto> CreateAsync(TripulanteRequestDto dto, CancellationToken ct = default)
    {
        if (!await _barcoRepository.ExistsAsync(dto.BarcoId, ct))
            throw new NotFoundException(nameof(Models.Barco), dto.BarcoId);

        var creado = await _tripulanteRepository.AddAsync(dto.ToEntity(), ct);
        return creado.ToDto();
    }

    public async Task<TripulanteDto> UpdateAsync(long id, TripulanteRequestDto dto, CancellationToken ct = default)
    {
        var tripulante = await _tripulanteRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Tripulante), id);

        if (!await _barcoRepository.ExistsAsync(dto.BarcoId, ct))
            throw new NotFoundException(nameof(Models.Barco), dto.BarcoId);

        tripulante.UpdateFromDto(dto);
        await _tripulanteRepository.UpdateAsync(tripulante, ct);
        return tripulante.ToDto();
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var tripulante = await _tripulanteRepository.FindByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Models.Tripulante), id);
        await _tripulanteRepository.DeleteAsync(tripulante, ct);
    }

    public async Task<List<TripulanteDto>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default) =>
        (await _tripulanteRepository.FindByBarcoIdAsync(barcoId, ct)).Select(t => t.ToDto()).ToList();
}