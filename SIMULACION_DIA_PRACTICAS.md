# Simulación: Día 3 de tu primera semana en Espiral MS

**Cómo usar este documento:** no es solo para leer. Cada bloque de hora tiene una acción real que puedes ejecutar (sobre tu proyecto MarinaApi) o una interacción simulada conmigo haciendo de compañero/a de equipo. Ve bloque a bloque, en orden, como si fuera un día real de trabajo.

**Contexto de partida (lo que ya ha pasado, días 1-2):**
- Día 1: te dieron acceso a Azure DevOps, Teams/Slack, VPN, y te presentaron al equipo
- Día 2: alguien te hizo un tour por la arquitectura general del producto, y ejecutaste el proyecto en local por primera vez (como ya hiciste tú de verdad con MarinaApi)
- Ya tienes asignado un **buddy** — llamémosle **Marcos**, un desarrollador con 3 años en la empresa — como tu referencia para dudas del día a día

---

## 09:00 — Llegada y Daily Standup

Te conectas, abres Teams. El equipo hace el daily de pie (o por videollamada si hay híbrido). Dura 10-15 minutos. Cada persona dice 3 cosas: qué hizo ayer, qué hará hoy, si tiene bloqueos.

**Tu turno** (es tu primer daily "real", no solo de observador):

> *"Ayer terminé de familiarizarme con el repositorio y levanté el entorno en local. Hoy voy a empezar con el ticket que me asignó [Scrum Master] — añadir un endpoint para filtrar barcos por capacidad mínima. Sin bloqueos por ahora."*

**Ejercicio:** escribe tú mismo, en un documento aparte, cómo dirías esas 3 cosas con tus propias palabras, adaptado a lo que de verdad hiciste ayer en MarinaApi. Practica decirlo en voz alta — la primera vez que hablas en un daily real da vértigo, y ensayarlo ayuda de verdad.

---

## 09:15 — Coges tu primer ticket

Marcos te pasa el enlace al ticket en Azure DevOps (simulado aquí como texto):

> **Ticket #142 — Filtrar barcos por capacidad mínima**
> *Como usuario de la API, quiero poder consultar los barcos con capacidad igual o superior a un valor dado, para poder planificar regatas según el número de tripulantes.*
>
> **Criterios de aceptación:**
> - Nuevo endpoint `GET /api/Barcos/capacidad/{minima}`
> - Devuelve solo barcos con `Capacidad >= minima`
> - Si no hay ninguno, devuelve una lista vacía (no error)
> - Debe tener al menos un test unitario
>
> **Story points:** 2

**Acción real:** abre tu proyecto MarinaApi en VS Code. Este es un ticket real que vas a implementar de verdad, siguiendo el flujo completo de Git que ya practicaste en el Capítulo 9 de tu guía.

```powershell
git checkout develop
git pull origin develop
git checkout -b feature/filtrar-barcos-capacidad
```

*(Si tu repo local de MarinaApi aún no tiene una rama `develop`, créala ahora desde `main` — en un proyecto real casi nunca trabajas directo sobre `main`.)*

---

## 09:30 — Implementas la tarea

Sigue el mismo patrón que ya usaste para `FindByEsloraGreaterThanAsync` — es prácticamente el mismo ticket con otro campo.

**Paso 1 — Repositorio.** Añade a `IBarcoRepository` y `BarcoRepository`:

```csharp
Task<List<Barco>> FindByCapacidadGreaterThanOrEqualAsync(int capacidadMinima, CancellationToken ct = default);
```

**Paso 2 — Servicio.** Añade a `IBarcoService` y `BarcoService`:

```csharp
Task<List<BarcoDto>> FindByCapacidadMinimaAsync(int capacidadMinima, CancellationToken ct = default);
```

**Paso 3 — Controller.** Añade a `BarcosController`:

```csharp
[HttpGet("capacidad/{minima:int}")]
[ProducesResponseType(typeof(List<BarcoDto>), StatusCodes.Status200OK)]
public async Task<ActionResult<List<BarcoDto>>> GetByCapacidadMinima(int minima, CancellationToken ct) =>
    Ok(await _barcoService.FindByCapacidadMinimaAsync(minima, ct));
```

**Acción real:** impleméntalo tú mismo en tu copia local del proyecto, sin copiar-pegar directamente — escríbelo de memoria apoyándote en el patrón de `FindByEsloraGreaterThanAsync` que ya existe. Es exactamente lo que harías el primer día real con un ticket similar.

---

## 10:15 — Te atascas en algo pequeño

Al escribir el test, no te acuerdas bien de la sintaxis exacta de Moq para mockear el nuevo método del repositorio. Llevas 10 minutos dándole vueltas.

**Esto es exactamente el momento de preguntar** — ni demasiado pronto (sin haberlo intentado) ni demasiado tarde (media hora bloqueado en silencio). Escríbele a Marcos por Teams:

> *"Oye Marcos, tengo una duda rápida — estoy mockeando `FindByCapacidadGreaterThanOrEqualAsync` en el repositorio para el test, pero no me acuerdo si `ReturnsAsync` acepta directamente una lista o hace falta algo más. ¿Tienes 2 minutos o prefieres que lo mire yo con calma?"*

**Simulación — así te respondería un compañero real:**

> *"Tranqui, con `ReturnsAsync(listaDeBarcos)` te vale directamente si ya tienes la lista construida. Mira el test de `FindAllAsync` que ya tienes en `BarcoServiceTests.cs`, es el mismo patrón. Si en 10 min sigues liado me dices y lo miramos juntos por videollamada."*

**Acción real:** escribe el test tú mismo ahora, en `MarinaApi.Tests/BarcoServiceTests.cs`, siguiendo el patrón de `FindAllAsync_MapeaTodasLasEntidadesADto` que ya tienes. Nómbralo `FindByCapacidadMinimaAsync_DevuelveBarcosFiltrados`.

---

## 11:00 — Pausa café / desconexión breve

En un trabajo real, esto también cuenta. Levántate 5-10 minutos. No es tiempo "perdido" — es parte normal del ritmo de una jornada de 8 horas, no de un sprint de estudio de 2 horas seguidas.

---

## 11:15 — Terminas la implementación, compilas y pruebas

```powershell
dotnet build
dotnet test
```

**Acción real:** ejecuta ambos comandos de verdad. Si `dotnet build` falla, ese es justo el tipo de error que resolverías tú solo (o preguntando) en un día real — no pasa nada, es parte del proceso. Si `dotnet test` te da la barra verde con tu test nuevo pasando, has completado la parte de código del ticket.

Prueba también el endpoint a mano en Swagger (`/swagger`), con un par de valores de capacidad, para confirmar que funciona de extremo a extremo — mismo paso que hiciste con el POST de barcos hace unos días.

---

## 12:00 — Preparas el Pull Request

```powershell
git add .
git commit -m "feat: añadir endpoint para filtrar barcos por capacidad mínima"
git push origin feature/filtrar-barcos-capacidad
```

**Acción real:** si tienes el repo de MarinaApi también en GitHub (o donde sea), crea el Pull Request de verdad — título, descripción breve de qué hace y por qué, enlaza el ticket. En Azure DevOps real, sería el mismo proceso: `feature/filtrar-barcos-capacidad` → PR contra `develop`.

**Descripción de PR realista, para que veas el formato:**

> **Título:** feat: filtrar barcos por capacidad mínima (#142)
>
> **Descripción:**
> Añade `GET /api/Barcos/capacidad/{minima}` para consultar barcos con capacidad >= al valor dado.
>
> - Nuevo método en `IBarcoRepository`/`BarcoRepository`
> - Nuevo método en `IBarcoService`/`BarcoService`
> - Nuevo endpoint en `BarcosController`
> - Test unitario en `BarcoServiceTests`
>
> Cierra #142

---

## 12:30 — Comida

Descanso real, sin pantalla si puedes. En muchas empresas es de 1h-1h30, según convenio. No forma parte del "trabajo productivo" pero sí de la jornada.

---

## 14:00 — Recibes el primer Code Review

Marcos revisa tu PR. Te deja comentarios (simulación realista de lo que te encontrarías):

> **Comentario 1** en `BarcosController.cs`, línea del nuevo endpoint:
> *"Buen trabajo. Una cosa: en el resto de endpoints del controller usamos `CancellationToken ct` como último parámetro siempre — aquí también lo tienes bien puesto. 👍"*
>
> **Comentario 2** en `BarcoRepository.cs`:
> *"¿Has probado qué pasa si `capacidadMinima` es negativo? No hace falta que lo valides aquí necesariamente, pero coméntalo por si el equipo de QA lo prueba — puede que quieran un `BadRequest` si mandan un número negativo. Dejo esto como comentario, no como bloqueante."*
>
> **Aprobado con comentario menor.**

**Esto es 100% realista.** Un PR de un junior casi nunca se aprueba sin ningún comentario — y eso es bueno, es cómo aprendes el estilo y los estándares del equipo. Un comentario "no bloqueante" significa que puedes mergear igualmente y abordarlo después si quieres, o en un ticket aparte.

**Acción real (opcional, para practicar aún más):** añade una comprobación en el controller — si `minima < 0`, devuelve `BadRequest()`. Es el tipo de ajuste rápido que harías tras un comentario de review real.

---

## 15:00 — Tarea distinta: te piden ayuda con algo urgente y pequeño

Un compañero de otro equipo (no el tuyo) pregunta en el canal general de Teams:

> *"¿Alguien sabe si el endpoint de exportar a PDF (Barco) ya está en producción o sigue en desarrollo?"*

Tú, como ya conoces el proyecto (aunque solo llevas 3 días), puedes contestar si lo sabes, o simplemente decir que no lo sabes y etiquetar a quien sí:

> *"No tengo esa info todavía, llevo pocos días — @Marcos, ¿tú sabrías decirle?"*

**Por qué incluyo esto:** parte real del día a día es este tipo de interrupciones pequeñas, y también aprender que **está bien decir "no lo sé, pero sé a quién preguntar"** — no se espera que un practicante de 3 días conozca todo el sistema.

---

## 15:15 — Retomas tu ticket, aplicas el comentario del review

```powershell
# aplicas el cambio pedido por Marcos
git add .
git commit -m "fix: validar capacidad mínima no negativa"
git push origin feature/filtrar-barcos-capacidad
```

**Acción real:** aplícalo de verdad en tu código, con el `BadRequest` si `minima < 0`.

---

## 16:00 — El PR se mergea

Marcos aprueba el cambio final, y el PR se mergea a `develop` (normalmente con "squash and merge", como viste en tu Lección 9).

```powershell
git checkout develop
git pull origin develop
git branch -d feature/filtrar-barcos-capacidad
```

**Sensación real:** tu primer ticket real, de principio a fin, cerrado. Esto es exactamente lo que sentirías el día que de verdad pase en Espiral MS.

---

## 16:15 — Actualizas el tablero

En Azure DevOps (o Jira, según el equipo), mueves el ticket #142 de "In Progress" a "Done" — o lo hace automáticamente al detectar que el PR se mergeó y menciona el ticket.

---

## 16:30 — Un rato de aprendizaje/documentación

No todo el día es ticket tras ticket. Es común que se reserve algo de tiempo (aunque sea 30-45 min) para leer documentación interna, ver cómo funciona una parte del sistema que aún no conoces, o simplemente consolidar lo aprendido.

**Acción real:** aprovecha este bloque para repasar el Capítulo 6 de tu `MIGRACION_JAVA_A_CSHARP.md` (Repositorio Genérico) — hoy has usado exactamente ese patrón en un ticket real, así que es el momento perfecto para que la teoría termine de encajar con la práctica que acabas de hacer.

---

## 17:15 — Cierre del día

Antes de desconectar, buena práctica (no obligatoria en todas las empresas, pero común): dejar una nota corta de qué queda para mañana, o actualizar el estado del ticket con un comentario breve.

> *"Ticket #142 completado y mergeado. Mañana empiezo con #148 (según lo que me indique el equipo)."*

---

## Reflexión final del ejercicio

Repasa mentalmente (o anótalo, si quieres) estas preguntas:

1. ¿En qué momento del día te habrías sentido más inseguro/a en la vida real?
2. ¿El ritmo de "ticket + interrupciones pequeñas + code review" encaja con lo que esperabas, o te imaginabas algo distinto?
3. ¿Qué parte técnica (Git, EF Core, tests) sentiste más sólida, y cuál menos?

Esto último es lo más útil del ejercicio: te dice dónde reforzar antes de que sea con código real de la empresa y con la presión real de un equipo esperando.

---

**Siguiente paso sugerido:** si quieres, la próxima vez montamos el **mini-sprint completo** (varios días, varios tickets, ceremonias de Scrum incluidas — Planning, dailies de varios días, Review, Retro) para practicar el ciclo completo, no solo un día suelto.
