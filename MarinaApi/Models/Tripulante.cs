using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Models;

/// <summary>
/// Representa un tripulante de un barco (nombre, rol, BarcoId)
/// Equivalente a la entidad Java "Tripulante". Es el lado propietario de la
/// relación N:1 con Barco (contiene la clave foránea BarcoId).
///
/// </summary>
public class Tripulante
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Rol { get; set; } = string.Empty;

    // ── Clave foránea explícita (equivalente a @JoinColumn(name = "barco_id")) ──
    public long BarcoId { get; set; }
    public Barco? Barco { get; set; }
}