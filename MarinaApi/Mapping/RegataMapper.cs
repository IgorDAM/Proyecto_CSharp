using MarinaApi.Dtos;
using MarinaApi.Models;
using System.Linq.Expressions;

namespace MarinaApi.Mapping;

public static class RegataMapper
{
    public static readonly Expression<Func<Regata, RegataDto>> ToDtoProjection =
    r => new RegataDto(
        r.Id, r.Nombre, r.Lugar, r.Fecha,
        r.Distancia, r.Barcos.Count);
    public static RegataDto ToDto(this Regata regata) => new(
        regata.Id, regata.Nombre, regata.Lugar, regata.Fecha,
        regata.Distancia, regata.Barcos.Count);

    public static Regata ToEntity(this RegataRequestDto dto) => new()
    {
        Nombre = dto.Nombre,
        Lugar = dto.Lugar,
        Fecha = dto.Fecha,
        Distancia = dto.Distancia
    };

    public static void UpdateFromDto(this Regata regata, RegataRequestDto dto)
    {
        regata.Nombre = dto.Nombre;
        regata.Lugar = dto.Lugar;
        regata.Fecha = dto.Fecha;
        regata.Distancia = dto.Distancia;
    }
}
