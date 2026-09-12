using Microsoft.EntityFrameworkCore;
using MarinaApi.Models;

namespace MarinaApi.Data;

/// <summary>
/// Equivalente C# de HibernateUtil + hibernate.cfg.xml + las anotaciones JPA
/// (@Entity, @OneToOne, @ManyToMany, @JoinTable) del proyecto Java, todo unificado
/// en una sola clase. EF Core llama a esto el "Fluent API": en vez de anotar cada
/// propiedad, configuramos las relaciones aquí de forma centralizada y explícita.
/// </summary>
public class MarinaDbContext : DbContext
{
    public MarinaDbContext(DbContextOptions<MarinaDbContext> options) : base(options) { }

    public DbSet<Barco> Barcos => Set<Barco>();
    public DbSet<Amarre> Amarres => Set<Amarre>();
    public DbSet<Regata> Regatas => Set<Regata>();
    public DbSet<Tripulante> Tripulantes => Set<Tripulante>();

    public DbSet<Organizador> Organizadores => Set<Organizador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Relación 1:1 Barco ↔ Amarre ──
        // Amarre es la entidad propietaria (tiene la FK BarcoId), igual que en
        // Java donde Amarre llevaba @OneToOne + @JoinColumn(name = "barco_id").
        modelBuilder.Entity<Amarre>()
            .HasOne(a => a.Barco)
            .WithOne(b => b.Amarre)
            .HasForeignKey<Amarre>(a => a.BarcoId)
            .OnDelete(DeleteBehavior.Cascade); // equivalente a cascade = CascadeType.ALL + orphanRemoval

        // ── Relación N:M Barco ↔ Regata ──
        // EF Core 5+ genera la tabla intermedia "BarcoRegata" automáticamente,
        // sin necesidad de una clase Java extra para representarla.
        modelBuilder.Entity<Barco>()
            .HasMany(b => b.Regatas)
            .WithMany(r => r.Barcos)
            .UsingEntity(j => j.ToTable("BarcoRegata"));

// ── Relación 1:N Barco ↔ Tripulante ──
// Tripulante es la entidad propietaria (tiene la FK BarcoId), igual que en
// Java donde Tripulante llevaba @ManyToOne + @JoinColumn(name = "barco_id").
// A diferencia de Barco↔Regata, esto NO genera tabla intermedia: la FK
// BarcoId vive directamente como columna en la tabla Tripulantes.
modelBuilder.Entity<Tripulante>()
    .HasOne(t => t.Barco)
    .WithMany(b => b.Tripulantes)
    .HasForeignKey(t => t.BarcoId)
    .OnDelete(DeleteBehavior.Cascade);

        // ── Relación 1:N Organizador ↔ Regata ──
        modelBuilder.Entity<Regata>()// Lado propietario: Regata tiene la FK OrganizadorId
            .HasOne(r => r.Organizador)// Lado propietario: Regata tiene la FK OrganizadorId
            .WithMany(o => o.Regatas)// Lado inverso: un organizador dirige muchas regatas
            .HasForeignKey(r => r.OrganizadorId)// Configura la FK en Regata
            .OnDelete(DeleteBehavior.SetNull);  // Si se borra organizador, Regata.OrganizadorId → NULL



        // ── Restricciones adicionales ──
        modelBuilder.Entity<Barco>().Property(b => b.Nombre).IsRequired();
        modelBuilder.Entity<Regata>().Property(r => r.Nombre).IsRequired();
    }
}
