using MarinaApi.Dtos;
using MarinaApi.Models;

namespace MarinaApi.Mapping;

public static class AmarreMapper
{
    public static AmarreDto ToDto(this Amarre amarre) => new(
        amarre.Id, amarre.Ubicacion, amarre.Precio, amarre.Profundidad,
        amarre.Longitud, amarre.Electricidad, amarre.BarcoId);

    public static Amarre ToEntity(this AmarreRequestDto dto) => new()
    {
        Ubicacion = dto.Ubicacion,
        Precio = dto.Precio,
        Profundidad = dto.Profundidad,
        Longitud = dto.Longitud,
        Electricidad = dto.Electricidad
    };

    public static void UpdateFromDto(this Amarre amarre, AmarreRequestDto dto)
    {
        amarre.Ubicacion = dto.Ubicacion;
        amarre.Precio = dto.Precio;
        amarre.Profundidad = dto.Profundidad;
        amarre.Longitud = dto.Longitud;
        amarre.Electricidad = dto.Electricidad;
    }
}
