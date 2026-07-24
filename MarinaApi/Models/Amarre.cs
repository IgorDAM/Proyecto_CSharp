using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Models;

/// <summary>
/// Representa un puesto de amarre del puerto.
/// Equivalente a la entidad Java "Amarre". Es el lado propietario de la
/// relación 1:1 con Barco (contiene la clave foránea BarcoId).
/// </summary>
public class Amarre
{
    public long Id { get; set; }

    [Required, MaxLength(20)]
    public string Ubicacion { get; set; } = string.Empty;

    public double Precio { get; set; }

    public int Profundidad { get; set; }

    public int Longitud { get; set; }

    public bool Electricidad { get; set; }

    // ── Clave foránea explícita (equivalente a @JoinColumn(name = "barco_id")) ──
    public long? BarcoId { get; set; }
    public Barco? Barco { get; set; }
}
