namespace MarinaApi.Dtos;

public record RegataDto(
    long Id,
    string Nombre,
    string Lugar,
    DateOnly Fecha,
    int Distancia,
    int TotalBarcosInscritos
);

public record RegataRequestDto(
    string Nombre,
    string Lugar,
    DateOnly Fecha,
    int Distancia
);
