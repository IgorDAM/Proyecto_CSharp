namespace MarinaApi.Dtos;

/// <summary>
///
/// BarcoNombre se añade (ticket #171, experimento 5) para que el frontend
/// muestre el nombre del barco sin un segundo fetch. Sigue sin meter el
/// Barco completo: solo un dato plano, así que no reaparece el bucle.
/// Es "string?" porque solo tiene valor si el Repository cargó la
/// navegación Barco (Include o proyección); si no, llega null.
/// </summary>
public record TripulanteDto(
    long Id,
    string Nombre,
    string Rol,
    long BarcoId,
    string? BarcoNombre
);

/// <summary>
/// DTO de entrada para crear/actualizar un tripulante.
///
/// A diferencia de AmarreRequestDto (donde BarcoId NO aparece, porque
/// Amarre.BarcoId es "long?" y la asignación a un barco se hace aparte),
/// aquí BarcoId sí forma parte del request: en el modelo, Tripulante.BarcoId
/// es "long" (no nullable) — un tripulante no puede existir sin barco desde
/// el momento en que se crea, así que el cliente tiene que indicar de qué
/// barco es ya en el alta. Si en el futuro añadimos una ruta anidada
/// (p.ej. POST /api/barcos/{barcoId}/tripulantes), el controller puede
/// simplemente ignorar/sobrescribir este campo con el id de la URL antes de
/// pasarle el DTO al service.
/// </summary>
public record TripulanteRequestDto(
    string Nombre,
    string Rol,
    long BarcoId
);
