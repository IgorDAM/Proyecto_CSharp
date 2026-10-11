# MarinaApi: documentación del repositorio

Este repositorio contiene **MarinaApi**, una Web API en ASP.NET Core 8 con EF Core, MySQL y tests con xUnit + Moq. Es un port a C# de un proyecto Java con Spring Boot e Hibernate, y se usa como campo de entrenamiento de C#/.NET.

Este archivo es el mapa: dice qué hay en cada documento y cuándo se actualiza. Para arrancar la API, mira `MarinaApi/README.md`.

## Documentos de estudio

| Documento | Qué es | Cuándo se actualiza |
|---|---|---|
| `GUIA_DEFINITIVA_CSHARP_1.md` | Fundamentos de C# por lecciones: tipos, LINQ, async/await, genéricos, patrones | Solo cuando aparece sintaxis o un concepto nuevo |
| `MIGRACION_JAVA_A_CSHARP.md` | Comparativa de Java/Spring Boot a C#/.NET, con ejemplos del proyecto | Solo cuando aparece sintaxis o un concepto nuevo |
| `GLOSARIO_CSHARP.md` | Términos que van saliendo en las sesiones, por tema: qué es, equivalente en Java y ejemplo | Cada vez que sale un término nuevo |

## Guías de herramientas

| Documento | Qué es | Cuándo se actualiza |
|---|---|---|
| `GUIA_GIT.md` | Git del día a día: commits, ramas, merge frente a rebase, flujo de Pull Request, deshacer cambios y conflictos | Cuando aparece una trampa o práctica nueva |
| `GUIA_CICD.md` | CI/CD con GitHub Actions: pipeline de MarinaApi, artifact, entornos y secretos, Docker, Azure y AWS | Cuando cambia el pipeline o se despliega algo nuevo |

## Proceso y seguimiento

| Documento | Qué es | Cuándo se actualiza |
|---|---|---|
| `BACKLOG_MARINAAPI.md` | Backlog técnico por prioridades, con el estado de cada ticket y el PR que lo cerró | Al cerrar o crear un ticket |
| `SIMULACION_SPRINT_COMPLETO.md` | Guion de un sprint Scrum completo con los tickets #150 a #153 | Nunca: es un guion que se sigue, no un diario |
| `SIMULACION_DIA_PRACTICAS.md` | Simulacro de un día suelto de prácticas, independiente del sprint | Nunca |
| `docs/informes/` | Informes con fecha en el nombre, como una foto del estado del proyecto en ese día | No se editan: se crea uno nuevo |
| `docs/research/` | Notas de investigación sobre temas concretos, por ejemplo RAG en .NET | Al investigar un tema nuevo |

## Por dónde empezar

1. Para arrancar la API y probarla en Swagger, sigue `MarinaApi/README.md`.
2. Para aprender la sintaxis, empieza por `GUIA_DEFINITIVA_CSHARP_1.md`.
3. Si vienes de Java y Spring Boot, `MIGRACION_JAVA_A_CSHARP.md` compara cada concepto con su equivalente.
4. Cuando una palabra no te suene, búscala en `GLOSARIO_CSHARP.md`.

## Espejo en el vault y convenciones

- Las tres guías de estudio (`GUIA_DEFINITIVA_CSHARP_1.md`, `MIGRACION_JAVA_A_CSHARP.md` y `GLOSARIO_CSHARP.md`) tienen una copia en el vault de Obsidian, en `Estudios/Programación/C#/`. Cada cambio en el repo se copia también al espejo.
- Entre documentos, las referencias usan wikilinks de Obsidian (`[[Glosario C#]]`), aunque en GitHub se vean como texto.
- Las referencias a archivos de código van en formato código, sin enlace.