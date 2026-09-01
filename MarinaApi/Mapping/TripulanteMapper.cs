using MarinaApi.Dtos;
using MarinaApi.Models;

namespace MarinaApi.Mapping;

/// <summary>
/// Equivalente a TripulanteMapper.java. Mismo patrón que BarcoMapper/AmarreMapper:
/// extension methods en vez de una clase con métodos estáticos.
/// </summary>
public static class TripulanteMapper
{
    public static TripulanteDto ToDto(this Tripulante tripulante) =>
        new(tripulante.Id, tripulante.Nombre, tripulante.Rol, tripulante.BarcoId);

    // Crea una entidad NUEVA (sin Id) a partir del request. BarcoId viaja
    // directamente porque, a diferencia de Amarre, un Tripulante siempre
    // nace ligado a un barco (ver comentario en TripulanteRequestDto).
    public static Tripulante ToEntity(this TripulanteRequestDto dto) => new()
    {
        Nombre = dto.Nombre,
        Rol = dto.Rol,
        BarcoId = dto.BarcoId
    };

    // Actualiza una entidad YA EXISTENTE (conserva Id). A diferencia de
    // BarcoMapper.UpdateFromDto, aquí SÍ tocamos la FK: permitimos reasignar
    // un tripulante a otro barco reutilizando el mismo endpoint de update,
    // igual que harías en Java con un simple setBarco(nuevoBarco).
    public static void UpdateFromDto(this Tripulante tripulante, TripulanteRequestDto dto)
    {
        tripulante.Nombre = dto.Nombre;
        tripulante.Rol = dto.Rol;
        tripulante.BarcoId = dto.BarcoId;
    }
}
