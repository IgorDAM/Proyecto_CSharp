# Informe de estado · MarinaApi → SEIDEL

**Fecha:** 30/09/2026 · **Periodo analizado:** 31/08 → 30/09/2026
**Generado con:** Claude (Claude Academy · Claude 101, lección 11) usando GitHub, Firecrawl, Claude in Chrome y Microsoft Learn.

> Fuentes: el repo [IgorDAM/Proyecto_CSharp](https://github.com/IgorDAM/Proyecto_CSharp), la web de SEIDEL ([transformación digital](https://seidelingenieria.com/transformacion-digital) y [porfolio](https://seidelingenieria.com/porfolio), con las fichas de cada proyecto abiertas) y la documentación de Microsoft Learn.

## Resumen

MarinaApi cubre el núcleo de lo que SEIDEL usa en sus ERPs: una API REST con .NET/C#, MySQL con EF Core y tests con xUnit+Moq. El mes ha sido muy productivo: 8 PRs fusionados y la migración a MySQL, el mismo motor que usan sus proyectos. Los huecos están alrededor del código: CI/CD con **Azure DevOps** (aparece en 4 de sus 6 proyectos .NET), despliegue en Azure, secretos fuera del repo, SOAP legacy y RAG.

**Próximos 3 pasos**

1. **#156:** sacar las credenciales de `appsettings.json` y pasarlas a `user-secrets`.
2. **Pipeline de build y tests en Azure DevOps** sobre el repo de GitHub.
3. **Bugs de datos #154 y #157:** el borrado en cascada del amarre y el `double` en el precio.

## Estado por área

| Área (según SEIDEL) | Estado | Evidencia | Hueco | Siguiente paso + recurso | Prioridad |
|---|---|---|---|---|---|
| .NET/C# + REST API | ✅ Hecho | [PR #1](https://github.com/IgorDAM/Proyecto_CSharp/pull/1) (CRUD de Tripulante), [PR #3](https://github.com/IgorDAM/Proyecto_CSharp/pull/3) (total de tripulantes) | Validación de entrada (#160), versionado (#169) | #160 con FluentValidation | Media |
| MySQL + EF Core | ✅ Hecho | [PR #4](https://github.com/IgorDAM/Proyecto_CSharp/pull/4) (migración a Pomelo), [PR #5](https://github.com/IgorDAM/Proyecto_CSharp/pull/5) y [#6](https://github.com/IgorDAM/Proyecto_CSharp/pull/6) (proyecciones, N+1 resuelto) | Cascade que borra el amarre (#154), `double` en dinero (#157) | #154, #157 · [Proveedores EF Core](https://learn.microsoft.com/ef/core/providers/) | **Alta** |
| Seguridad / configuración | ⚠️ En progreso | La cadena de conexión de MySQL con contraseña sigue versionada en `MarinaApi/appsettings.json`; el log de SQL está en `Information` en todos los entornos | #156 (el texto del ticket aún habla de SQL Server), #162 | [Almacenamiento seguro de secretos](https://learn.microsoft.com/aspnet/core/security/app-secrets) | **Alta** |
| Tests (xUnit + Moq) | ✅ Hecho | 35 tests en verde ([PR #4](https://github.com/IgorDAM/Proyecto_CSharp/pull/4)); los 3 caminos de error de AssignBarco ([PR #2](https://github.com/IgorDAM/Proyecto_CSharp/pull/2)) | #159 parece resuelto por el PR #2 pero sigue en ⬜; faltan tests de integración (#161) | Marcar #159 y seguir con `WebApplicationFactory` | Media |
| CI/CD · Azure DevOps | ⬜ No empezado | Solo `GUIA_CICD.md` (escrita para GitHub Actions); no hay `.github/workflows` ni `azure-pipelines.yml` | SEIDEL indica Azure DevOps en Rutas, Laboratorios Dentales, PMS y Eventos Deportivos | `azure-pipelines.yml` con restore, build y test · [Build, test y deploy de .NET](https://learn.microsoft.com/azure/devops/pipelines/ecosystems/dotnet-core) | **Alta** |
| Despliegue en Azure | ⬜ Sin evidencia en el repo | Solo `docker-compose.yml` local | La web menciona Azure y AWS | App Service + MySQL Flexible Server · [Tutorial](https://learn.microsoft.com/azure/mysql/flexible-server/tutorial-webapp-server-vnet) | Media |
| Errores / ProblemDetails | ⚠️ En progreso | `Middleware/ExceptionHandlingMiddleware.cs` hecho a mano | #163 | [IExceptionHandler + ProblemDetails](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling) | Media |
| PostgreSQL | ⬜ Sin evidencia en el repo | Solo teoría (lección 7 de la guía) | Lo usan en Bluesite (React/Node) | Probar Npgsql en una rama · [Proveedores EF Core](https://learn.microsoft.com/ef/core/providers/) | Media |
| MongoDB | ⬜ Sin evidencia en el repo | — | Aparece en el ERP de Rutas | `MongoDB.EntityFrameworkCore` (misma página de proveedores) | Baja |
| SOAP/XML legacy | ⬜ Sin evidencia en el repo | — | La web lo confirma: "SOAP y XML cuando el proceso es veterano" | [CoreWCF](https://learn.microsoft.com/dotnet/core/porting/upgrade-assistant-wcf) | Media |
| IA / RAG (LUMINA) | ⚠️ Solo teoría | Informe de Research en `docs/research` ([ce5ee7f](https://github.com/IgorDAM/Proyecto_CSharp/commit/ce5ee7fc73a377100d78b12cd58422242f9977af)) | LUMINA: LLM, RAG, NLP, integración de datos y Microsoft 365 | Mini buscador vectorial en .NET · [Vector databases for .NET AI apps](https://learn.microsoft.com/dotnet/ai/vector-stores/overview) | Media |
| Preparación para producción | ⬜ No empezado | — | Health checks y CORS (#167, #168) | [Health checks + AddDbContextCheck](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks) | Baja |

**Tecnologías del porfolio de SEIDEL que MarinaApi no cubre (solo para tenerlas en el radar):** frontend con Bootstrap/HTML/JS, escritorio con WPF y Windows Forms, una app Android, Power BI y Python (buscador Gaia DR3).

## Actividad del repo (31/08 → 30/09)

19 commits y 8 PRs fusionados. Todo es de septiembre; en la última semana de agosto no hubo actividad.

- **12/09:** CRUD de Tripulante (PR #1) y limpieza de bin/obj con `.gitignore`.
- **13/09 y 15/09:** asignar Amarre a Barco con validación de conflicto (#151) y sus tests (PR #2); lecciones 11-13 de la guía; backlog #154-#170; cambio de Espiral MS a SEIDEL.
- **21/09 y 22/09:** total de tripulantes por regata (PR #3), migración a MySQL con Pomelo (PR #4), guías de Git y CI/CD.
- **23/09:** informe de Research sobre RAG en .NET y guía de migración actualizada.
- **26/09 y 27/09:** fix de `totalBarcosInscritos` con proyección a DTO (PR #5), N+1 resuelto (PR #6), SqlServer eliminado (PR #7) y glosario ampliado (PR #8).

No hay issues en GitHub: el backlog vive en `BACKLOG_MARINAAPI.md`.

## Notas de método

- La web de SEIDEL solo muestra 4 tecnologías por proyecto (el resto queda tras "+4" o "+6"). La lista completa sale al abrir cada ficha. Así aparecieron Azure DevOps, MongoDB, WPF y Microsoft 365.
- Regla aplicada: si un área no tiene evidencia en el repo, se indica "sin evidencia" en vez de suponer nada.
