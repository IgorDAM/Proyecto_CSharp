namespace MarinaApi.Dtos;

/// <summary>
/// DTO completo de Tripulante para la API REST. Igual que en BarcoDto, el
/// "record" nos ahorra escribir a mano Equals/GetHashCode/ToString y el
/// constructor (en Java, Lombok @Data + @AllArgsConstructor).
///
/// Se expone BarcoId (long) en vez del objeto Barco completo. Motivo: el
/// modelo Tripulante tiene navegación de vuelta a Barco, y Barco tiene la
/// lista de Tripulantes — si el DTO metiera aquí un BarcoDto completo con
/// su lista de tripulantes, tendríamos el mismo problema que un
/// @JsonManagedReference/@JsonBackReference mal puesto en Java: sin ellos,
/// Jackson (o System.Text.Json aquí) entra en bucle infinito serializando
/// Barco → Tripulantes → Barco → ... Al aplanar la relación a un simple Id
/// evitamos el problema de raíz, sin depender de atributos de serialización.
/// </summary>
public record TripulanteDto(
    long Id,
    string Nombre,
    string Rol,
    long BarcoId
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
