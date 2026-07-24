namespace MarinaApi.Dtos;

/// <summary>
/// DTO completo de Barco para la API REST. Equivalente a BarcoDTO.java.
/// Un "record" en C# es la forma idiomática de escribir un DTO inmutable:
/// genera automáticamente el equivalente a los @Data + @AllArgsConstructor
/// de Lombok (igualdad por valor, ToString, constructor) sin escribir nada más.
/// </summary>
public record BarcoDto(
    long Id,
    string Nombre,
    string Tipo,
    int Eslora,
    int Manga,
    int Capacidad
);

/// <summary>
/// DTO de entrada para crear/actualizar un barco. Se separa del DTO de salida
/// porque el cliente nunca debe poder fijar el Id (lo genera la base de datos) —
/// una mejora respecto al proyecto Java, que reutilizaba el mismo BarcoDTO
/// tanto para petición como para respuesta.
/// </summary>
public record BarcoRequestDto(
    string Nombre,
    string Tipo,
    int Eslora,
    int Manga,
    int Capacidad
);

/// <summary>
/// DTO resumido para listados. Equivalente a BarcoSummaryDTO.java.
/// </summary>
public record BarcoSummaryDto(
    long Id,
    string Nombre,
    string Tipo
);
