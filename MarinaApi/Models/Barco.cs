using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Models;

/// <summary>
/// Representa un barco atracado en el puerto.
/// Equivalente a la entidad Java "Barco" (Cap. 4-5 del tutorial de Hibernate),
/// migrada a Entity Framework Core.
/// </summary>
public class Barco
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Tipo { get; set; } = string.Empty;

    public int Eslora { get; set; }

    public int Manga { get; set; }

    public int Capacidad { get; set; }

    // ── Relación 1:1 con Amarre ──
    // En Java, Barco era el "lado inverso" (mappedBy = "barco").
    // En EF Core no existe mappedBy: la relación se configura en el DbContext
    // (Fluent API, ver MarinaDbContext.OnModelCreating). Aquí solo declaramos
    // la propiedad de navegación.
    public Amarre? Amarre { get; set; }

    // ── Relación N:M con Regata ──
    // EF Core 5+ soporta "skip navigations": ya NO hace falta crear una clase
    // Java para la tabla intermedia "barco_regata". EF Core la gestiona sola
    // a partir de esta lista, igual que hacía @ManyToMany + @JoinTable en Java,
    // pero sin anotaciones ni tabla intermedia explícita en el código.
    public List<Regata> Regatas { get; set; } = new();

    // ── Relación 1:N con Tripulante ──
    // EF Core 5+ soporta "skip navigations": ya NO hace falta crear
    // una clase Java para la tabla intermedia "barco_tripulante". EF Core la
    // gestiona sola a partir de esta lista, igual que hacía @OneToMany + @JoinColumn
    // en Java, pero sin anotaciones ni tabla intermedia explícita en el código.
    public List<Tripulante> Tripulantes { get; set; } = new();
}
