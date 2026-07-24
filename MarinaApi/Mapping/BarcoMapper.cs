using MarinaApi.Dtos;
using MarinaApi.Models;

namespace MarinaApi.Mapping;

/// <summary>
/// Equivalente a BarcoMapper.java, pero implementado como extension methods
/// en vez de métodos estáticos con nombre de clase repetido. En C# esto permite
/// escribir "barco.ToDto()" en vez de "BarcoMapper.toDTO(barco)" — más legible
/// y encadenable con LINQ, ej: barcos.Select(b => b.ToDto()).
/// </summary>
public static class BarcoMapper
{
    public static BarcoDto ToDto(this Barco barco) =>
        new(barco.Id, barco.Nombre, barco.Tipo, barco.Eslora, barco.Manga, barco.Capacidad);

    public static BarcoSummaryDto ToSummaryDto(this Barco barco) =>
        new(barco.Id, barco.Nombre, barco.Tipo);

    // Equivalente a BarcoMapper.toEntity(dto): crea una entidad NUEVA (sin Id).
    public static Barco ToEntity(this BarcoRequestDto dto) => new()
    {
        Nombre = dto.Nombre,
        Tipo = dto.Tipo,
        Eslora = dto.Eslora,
        Manga = dto.Manga,
        Capacidad = dto.Capacidad
    };

    // Equivalente a BarcoMapper.updateEntityFromDTO(dto, barco): actualiza una
    // entidad YA EXISTENTE (conserva Id, Amarre y Regatas intactos).
    public static void UpdateFromDto(this Barco barco, BarcoRequestDto dto)
    {
        barco.Nombre = dto.Nombre;
        barco.Tipo = dto.Tipo;
        barco.Eslora = dto.Eslora;
        barco.Manga = dto.Manga;
        barco.Capacidad = dto.Capacidad;
    }
}
