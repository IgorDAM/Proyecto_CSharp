using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Models;

public class Organizador
{
    public long Id { get; set; }

    //   equivalente a @NotNull + @Size(max = 100) en Java   
    
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    // Lado inverso: un organizador dirige muchas regatas
    public List<Regata> Regatas { get; set; } = new();
}