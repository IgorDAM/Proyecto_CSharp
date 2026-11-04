namespace MarinaApi.Dtos;

public record AmarreDto(
    long Id,
    string Ubicacion,
    decimal Precio,
    int Profundidad,
    int Longitud,
    bool Electricidad,
    long? BarcoId
);

public record AmarreRequestDto(
    string Ubicacion,
    decimal Precio,
    int Profundidad,
    int Longitud,
    bool Electricidad
);

public record AsignarBarcoDto(long BarcoId);
