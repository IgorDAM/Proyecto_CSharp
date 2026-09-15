# Simulación: Tu primer Sprint completo de prácticas

**Cómo usar este documento:** simula una semana completa de sprint (Lunes-Viernes), con las ceremonias reales de Scrum. A diferencia de la simulación del "día 3", aquí el foco no es el detalle hora a hora, sino **el ciclo completo**: cómo se planifica, cómo evoluciona el trabajo día a día, qué pasa cuando algo se tuerce, y cómo se cierra. Cada ticket es una tarea real que vas a implementar en tu proyecto MarinaApi.

**Equipo simulado:** tú, **Marcos** (tu buddy, dev senior), **Laura** (Scrum Master), **Elena** (Product Owner).

---

## LUNES — Sprint Planning

**09:00.** El equipo se reúne (presencial o por videollamada). Elena (PO) trae el backlog priorizado. Entre todos se decide qué entra en el sprint de esta semana.

**Elena (PO):**
> *"Esta semana necesitamos poder gestionar la tripulación de los barcos — ahora mismo la entidad `Tripulante` existe en el modelo pero no hay forma de consultarla ni crearla desde la API. También hay una petición de que se pueda asignar un amarre a un barco directamente, y una regata necesita mostrar cuántos tripulantes totales están inscritos."*

Entre todos, desglosáis el trabajo en tickets y los estimáis con Planning Poker (como viste en la Lección 10):

| Ticket | Descripción | Estimación inicial (cada uno) | Consenso |
|---|---|---|---|
| **#150** | CRUD completo de Tripulante (Repository + Service + Controller) | Tú: 5 · Marcos: 8 | Se discute: tú pensabas que era solo el Controller, Marcos señala que también hace falta el Repository y Service — **8 puntos** |
| **#151** | Endpoint para asignar un Amarre a un Barco existente | Tú: 3 · Marcos: 3 | **3 puntos** |
| **#152** | Endpoint que devuelva el total de tripulantes inscritos en una regata (sumando los de todos sus barcos) | Tú: 2 · Marcos: 5 | Se discute: parece simple pero hay que recorrer la relación N:M Barco↔Regata y luego los tripulantes de cada barco — **5 puntos** |
| **#153** | Bug: `GET /api/Amarres/libres` devuelve error 500 si no hay ningún amarre libre | Tú: 1 · Marcos: 1 | **1 punto** |

**Total del sprint: 17 puntos.**

> **Nota realista:** como es tu primer sprint, es normal que el equipo te asigne principalmente **#150 y #153**, y que Marcos se quede con #151 y #152 (o los hagáis juntos) — nadie espera que un practicante de primera semana cargue con todo el sprint. Aquí, para que practiques lo máximo posible, **vas a implementar los 4**, pero en la vida real tu carga sería menor.

**Acción real:** antes de seguir, crea en tu proyecto una rama nueva desde `develop` para cada ticket, según los vayas abordando (no las crees todas ahora, se hace bajo demanda, según tu Lección 9).

---

## LUNES — Daily (tarde, como cierre simbólico del día de planning)

No suele haber daily el mismo día del Planning (ya habéis hablado suficiente), pero algunos equipos sí lo hacen breve. Si tu equipo lo hace:

> **Tú:** *"Hoy ha sido Planning, cojo el #150 mañana a primera hora."*

---

## MARTES — Ticket #150: CRUD de Tripulante

**09:00 — Daily.**
> **Tú:** *"Ayer terminamos el Planning. Hoy empiezo con el #150, el CRUD de Tripulante. Sin bloqueos."*

**Acción real — implementa el ticket completo:**

**1. Repositorio** (`Repositories/TripulanteRepository.cs`), siguiendo el patrón exacto de `AmarreRepository`:

```csharp
public interface ITripulanteRepository : IGenericRepository<Tripulante>
{
    Task<List<Tripulante>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default);
}

public class TripulanteRepository : GenericRepository<Tripulante>, ITripulanteRepository
{
    private readonly MarinaDbContext _context;

    public TripulanteRepository(MarinaDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Tripulante>> FindByBarcoIdAsync(long barcoId, CancellationToken ct = default) =>
        await _context.Tripulantes.Where(t => t.BarcoId == barcoId).ToListAsync(ct);
}
```

**2. DTOs** (`Dtos/TripulanteDtos.cs`):

```csharp
public record TripulanteDto(long Id, string Nombre, string Rol, long BarcoId);
public record TripulanteRequestDto(string Nombre, string Rol, long BarcoId);
```

**3. Mapper** (`Mapping/TripulanteMapper.cs`):

```csharp
public static class TripulanteMapper
{
    public static TripulanteDto ToDto(this Tripulante t) => new(t.Id, t.Nombre, t.Rol, t.BarcoId);

    public static Tripulante ToEntity(this TripulanteRequestDto dto) => new()
    {
        Nombre = dto.Nombre,
        Rol = dto.Rol,
        BarcoId = dto.BarcoId
    };
}
```

**4. Servicio** (`Services/TripulanteService.cs`) — implementa tú mismo `ITripulanteService`/`TripulanteService` con `FindAllAsync`, `FindByIdAsync`, `CreateAsync`, `DeleteAsync`, siguiendo exactamente el patrón de `AmarreService` que ya tienes. Es una buena prueba de si el patrón se te ha quedado interiorizado sin mirar el original.

**5. Controller** (`Controllers/TripulantesController.cs`) — también tú mismo, mismo patrón que `AmarresController`.

**6. Registro en `Program.cs`:**
```csharp
builder.Services.AddScoped<ITripulanteRepository, TripulanteRepository>();
builder.Services.AddScoped<ITripulanteService, TripulanteService>();
```

**17:30 — Cierras el día.** El ticket #150 probablemente no lo termines en un solo día (son 8 puntos, ~2 días de trabajo real) — y eso es completamente normal. No pasa nada por dejar un ticket "en progreso" de un día para otro.

---

## MIÉRCOLES — Terminas #150, aparece un bloqueo real

**09:00 — Daily.**
> **Tú:** *"Ayer avancé bastante con el #150, me falta terminar el Controller y probarlo en Swagger. Hoy lo termino y si me da tiempo empiezo el #153."*

**10:30 — Bloqueo real.** Al generar la migración, te sale un error:

```powershell
dotnet ef migrations add AnadirTripulanteEndpoints
```

Si te devuelve algo como *"Unable to create an object of type 'MarinaDbContext'"* o similar, es un error real que **puedes encontrarte de verdad** al hacerlo. Este es exactamente el momento de aplicar lo del ejercicio anterior: inténtalo 10-15 minutos tú, y si sigue sin salir, trátalo como si me preguntaras a mí en el chat real (o pregúntame de verdad si te pasa).

**Simulación de cómo respondería Marcos:**
> *"¿Has comprobado que el DbContext tiene un constructor sin parámetros o un `IDesignTimeDbContextFactory`? A veces con inyección de dependencias `dotnet ef` no sabe cómo instanciar el contexto fuera de la app. Prueba `dotnet ef migrations add ... --startup-project MarinaApi` si tienes varios proyectos, o revisa que `Program.cs` compile bien primero con `dotnet build`."*

**Acción real:** termina el ticket, aplica la migración, y pruébalo en Swagger creando 2-3 tripulantes para un barco que ya tengas creado.

**17:00 — Marcas #150 como terminado**, abres el PR, lo dejas listo para review.

---

## JUEVES — Tickets #151 y #153

**09:00 — Daily.**
> **Tú:** *"Ayer terminé el #150 y ya está en review. Hoy voy a por el #151 (asignar amarre a barco) y si me sobra tiempo, el bug #153."*

**Marcos, en el daily:**
> *"Vale, en cuanto tenga un hueco te reviso el PR del #150. Para el #151, ojo: recuerda que la relación Barco↔Amarre es 1:1, así que si el barco ya tiene amarre asignado, tendrás que decidir qué hacer — ¿sobrescribir, o devolver error?"*

Esa pregunta de Marcos es real y buena — **tienes que tomar una decisión de diseño**, no solo escribir código. Vamos a resolverla juntos aquí:

**Decisión para #151:** si el barco ya tiene amarre, el endpoint debe devolver un error (no sobrescribir en silencio) — es más seguro y explícito. El cliente de la API puede decidir desasignar primero si de verdad quiere cambiarlo.

**Acción real — implementa el endpoint:**

```csharp
// En AmarresController.cs
[HttpPut("{amarreId:long}/asignar-barco/{barcoId:long}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<IActionResult> AsignarBarco(long amarreId, long barcoId, CancellationToken ct)
{
    await _amarreService.AsignarBarcoAsync(amarreId, barcoId, ct);
    return NoContent();
}
```

Y en el servicio, añade la lógica que valida si el barco ya tiene amarre (pista: usa una excepción de dominio nueva, tipo `ConflictException`, y añade su manejo al middleware — igual que hiciste con `NotFoundException` en su momento).

**Por la tarde**, abordas el bug **#153** (`GET /api/Amarres/libres` fallando con 500 si no hay ninguno). Investígalo: probablemente sea un `.First()` en vez de `.FirstOrDefault()` en algún sitio, o algo similar — revisa `FindLibresAsync` en `AmarreRepository`.

**17:30.** Ambos tickets, PR abiertos.

---

## VIERNES — Ticket #152, Sprint Review y Retrospectiva

**09:00 — Daily** (el último del sprint).
> **Tú:** *"Ayer terminé el #151 y el bug #153, ambos en review. Hoy por la mañana hago el #152 (total de tripulantes por regata) para llegar a la Review con todo cerrado."*

**Acción real — ticket #152**, el más interesante técnicamente porque cruza dos relaciones:

```csharp
// En RegataService.cs
public async Task<int> ContarTripulantesTotalesAsync(long regataId, CancellationToken ct = default)
{
    var regata = await _regataRepository.FindByIdWithBarcosAsync(regataId, ct)
        ?? throw new NotFoundException(nameof(Models.Regata), regataId);

    var totalTripulantes = 0;
    foreach (var barco in regata.Barcos)
    {
        var tripulantes = await _tripulanteRepository.FindByBarcoIdAsync(barco.Id, ct);
        totalTripulantes += tripulantes.Count;
    }
    return totalTripulantes;
}
```

*(Necesitarás inyectar `ITripulanteRepository` en `RegataService` para esto — practica tú mismo cómo se hace, ya lo has hecho varias veces.)*

Añade el endpoint correspondiente en `RegatasController`: `GET /api/Regatas/{id}/tripulantes-totales`.

---

### 15:00 — Sprint Review

Todo el equipo (incluida Elena, la PO) se reúne para ver una **demo en vivo** de lo que se ha completado — nada de diapositivas, software funcionando de verdad.

**Tú, haciendo la demo de tus tickets** (practícalo, aunque sea solo para ti, en voz alta):

> *"Este sprint hemos añadido gestión completa de tripulantes. Os enseño: creo un tripulante para el barco 'Estrella del Mar'... [lo haces en Swagger, en vivo]... y ahora consulto la lista de tripulantes de ese barco. También hemos añadido la asignación de amarres a barcos, con control de que no se pueda asignar dos veces sin desasignar antes. Y el conteo de tripulantes totales por regata, que suma automáticamente los de todos los barcos inscritos."*

**Elena (PO):**
> *"Genial, justo lo que necesitábamos. Una pregunta: cuando un barco se desinscribe de una regata, ¿el conteo se actualiza solo?"*

**Tú:**
> *"Sí, porque siempre consultamos en el momento, no guardamos un número fijo — así que se actualiza automáticamente."*

Esto es real: **la Review también sirve para que te hagan preguntas que no habías pensado**, y a veces descubres ahí mismo un caso límite que se te había pasado.

---

### 16:00 — Sprint Retrospective

Solo el equipo de desarrollo (sin la PO normalmente). Reflexionáis sobre el **proceso**, no sobre el producto.

**Laura (Scrum Master):** *"¿Qué salió bien esta semana?"*

> **Tú:** *"Para ser mi primer sprint, me ayudó mucho tener el patrón ya hecho en otras entidades — reutilizar la estructura de Amarre para hacer Tripulante fue mucho más rápido que empezar de cero."*
>
> **Marcos:** *"Y la comunicación fue buena, preguntaste cuando te atascaste en vez de perder media hora en silencio."*

**Laura:** *"¿Qué no salió tan bien?"*

> **Tú:** *"Subestimé el #150 al principio — pensé que era solo el Controller, y era Repository + Service + Controller + DTOs + Mapper. La próxima vez lo desgloso mejor antes de estimar."*

**Laura:** *"¿Qué mejoramos para el próximo sprint?"*

> *"Que en el Planning, cuando alguien nuevo estime, recordemos explícitamente contar todas las capas (Repository, Service, Controller, tests), no solo la parte más visible."*

Esta última frase se convierte en una **acción concreta** para el siguiente sprint — así funciona una Retro de verdad, no es solo desahogo, produce mejoras accionables.

---

## Resumen del sprint

| Ticket | Estado | Puntos |
|---|---|---|
| #150 — CRUD Tripulante | ✅ Completado | 8 |
| #151 — Asignar amarre a barco | ✅ Completado | 3 |
| #152 — Total tripulantes por regata | ✅ Completado | 5 |
| #153 — Bug amarres libres | ✅ Completado | 1 |

**Velocidad del sprint: 17 puntos completados de 17 comprometidos.** (En la realidad, completar el 100% en tu primer sprint sería excepcionalmente bueno — no te frustres si en la práctica real avanzas más despacio; la estimación mejora con la experiencia del equipo, como viste en el concepto de "Velocity" de la Lección 10.)

---

## Reflexión final

1. ¿En qué ticket sentiste que ya dominabas el patrón sin necesitar mirar ejemplos?
2. La decisión de diseño del #151 (¿sobrescribir o dar error?) — ¿la habrías resuelto igual tú solo, o te habría hecho falta preguntar?
3. ¿Cómo te sentiste haciendo la demo de la Review, aunque fuera simulada? Es una habilidad que se entrena, no algo que sale bien a la primera.

---

**Con esto tienes MarinaApi bastante más completo que al principio** — de 3 entidades a 4, con una relación 1:N nueva, un endpoint de asignación con lógica de conflicto, y un endpoint que cruza dos relaciones distintas. Es un buen momento para hacer commit final de todo y, si quieres, retomar el **Capítulo 5 de la guía** (Estados y Change Tracking) que quedó pendiente.
