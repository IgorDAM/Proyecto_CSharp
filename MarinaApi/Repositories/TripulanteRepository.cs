using Microsoft.EntityFrameworkCore;
using MarinaApi.Data;
using MarinaApi.Models;

namespace MarinaApi.Repositories;

public interface ITripulanteRepository : IGenericRepository<Tripulante>
{
    Task<List<Tripulante>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);

    Task<List<Tripulante>> FindAllWithBarcoAsync(CancellationToken ct = default);
    Task<(List<Tripulante> Elementos, int Total)> FindPageWithBarcoAsync(int pagina, int tamanio, CancellationToken ct = default);
}


public class TripulanteRepository : GenericRepository<Tripulante>, ITripulanteRepository
{
    private readonly MarinaDbContext _context;

    public TripulanteRepository(MarinaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Tripulante>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default) =>
        await _context.Tripulantes.Where(t => t.BarcoId == barcoId).ToListAsync(ct);


    // Include = JOIN FETCH de JPQL: carga la navegación Barco en la misma
    // consulta. Sin él, EF Core deja Tripulante.Barco a null (no hay lazy loading).
    public async Task<List<Tripulante>> FindAllWithBarcoAsync(CancellationToken ct = default) =>
        await _context.Tripulantes
            .Include(t => t.Barco)
            .ToListAsync(ct);

    // Misma consulta que FindAllWithBarcoAsync (Include = JOIN FETCH), pero paginada.
    // Con el orden explícito por Id también se arregla aquí el orden indefinido del
    // listado completo (#180): PaginarAsync siempre lleva ORDER BY.
    public async Task<(List<Tripulante> Elementos, int Total)> FindPageWithBarcoAsync(
        int pagina, int tamanio, CancellationToken ct = default) =>
        await _context.Tripulantes
            .Include(t => t.Barco)
            .PaginarAsync(t => t.Id, pagina, tamanio, ct);
}