namespace MarinaApi.Dtos;

public record AmarreDto(
    long Id,
    string Ubicacion,
    double Precio,
    int Profundidad,
    int Longitud,
    bool Electricidad,
    long? BarcoId
);

public record AmarreRequestDto(
    string Ubicacion,
    double Precio,
    int Profundidad,
    int Longitud,
    bool Electricidad
);

public record AsignarBarcoDto(long BarcoId);
