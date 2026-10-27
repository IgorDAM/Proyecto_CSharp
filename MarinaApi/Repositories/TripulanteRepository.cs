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
        await _context.Tripulantes.Where(t => t.BarcoId == barcoId).OrderBy(t => t.Id).ToListAsync(ct);


    // Include = JOIN FETCH de JPQL: carga la navegación Barco en la misma
    // consulta. Sin él, EF Core deja Tripulante.Barco a null (no hay lazy loading).
    // OrderBy(t => t.Id) = "order by t.id" de JPQL: sin ORDER BY, SQL no garantiza
    // el orden de las filas (#180). Con ORDER BY, FindAllWithBarcoAsync devuelve siempre el mismo orden.
    public async Task<List<Tripulante>> FindAllWithBarcoAsync(CancellationToken ct = default) =>
        await _context.Tripulantes
            .Include(t => t.Barco)
            .OrderBy(t => t.Id) // #180: orden explícito por Id para que FindAllWithBarcoAsync devuelva siempre el mismo orden
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