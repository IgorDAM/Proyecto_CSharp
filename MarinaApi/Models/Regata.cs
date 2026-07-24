using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Models;

/// <summary>
/// Representa una regata (competición de barcos).
/// Equivalente a la entidad Java "Regata".
/// </summary>
public class Regata
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Lugar { get; set; } = string.Empty;

    // Equivalente a @Temporal(TemporalType.DATE): solo guardamos la fecha, sin hora.
    public DateOnly Fecha { get; set; }

    public int Distancia { get; set; }

    // Lado inverso de la relación N:M (equivalente a @ManyToMany(mappedBy = "regatas")).
    public List<Barco> Barcos { get; set; } = new();

    // Lado inverso de la relación 1:N (equivalente a @ManyToOne + @JoinColumn(name = "organizador_id")).
    public long? OrganizadorId { get; set; }// FK hacia Organizador (nullable porque puede no tener organizador)

    public Organizador? Organizador { get; set; }// Lado inverso: una regata tiene un organizador
}
