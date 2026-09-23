> Generado con Claude Research el 23/09/2026 (Claude 101, lección 10). Fuentes de 2025-2026: revisar versiones en NuGet antes de usar.

# Plataforma LLM + RAG sobre datos propios con .NET/C#: arquitectura, librerías, bases vectoriales y ruta de aprendizaje para Igor (2026-2027)

Para construir hoy un RAG en .NET, la base recomendada es **.NET 10 LTS + Microsoft.Extensions.AI (IChatClient/IEmbeddingGenerator) + Microsoft.Extensions.VectorData con un conector CommunityToolkit.VectorData + PostgreSQL con pgvector**. Si necesitas agentes u orquestación, añade Microsoft Agent Framework 1.0, que ya es GA. Para proyectos nuevos, Kernel Memory y los nombres antiguos `Microsoft.SemanticKernel.Connectors.*` deben evitarse.

## TL;DR

- **Pila recomendada (septiembre 2026):**
  - Microsoft.Extensions.AI y Microsoft.Extensions.VectorData, que según el .NET Blog son GA desde el 21 de mayo de 2025, para hablar con modelos y almacenes vectoriales. Ambos van ya por la versión 10.10.0.
  - Los conectores `CommunityToolkit.VectorData.*` (v1 estable desde julio de 2026) para la base de datos.
  - Microsoft.Extensions.DataIngestion (**todavía preview**) para leer y trocear documentos.
  - Microsoft Agent Framework 1.0, sucesor de Semantic Kernel, si hay agentes. Según su blog oficial es GA desde el 2 de abril de 2026.
  - Kernel Memory es un proyecto de investigación archivado y sus paquetes están deprecados.
- **Base vectorial:** con el stack de la consultora (PostgreSQL/MySQL, Azure/AWS), la opción más natural es **PostgreSQL + pgvector**. Funciona en local, en Azure Database for PostgreSQL (que añade DiskANN, GA) y en Aurora/RDS PostgreSQL, y el mismo conector .NET sirve en los tres casos. **MySQL Community no sirve para búsqueda vectorial**: la función `DISTANCE()` solo existe en HeatWave en OCI y MySQL AI. MariaDB 11.8 LTS sí tiene vectores nativos. Como servicios gestionados, las alternativas son Azure AI Search y Bedrock Knowledge Bases con S3 Vectors, que según AWS es GA desde el 2 de diciembre de 2025.
- **Para Igor:** .NET 8 y .NET 9 **dejan de tener soporte el 10 de noviembre de 2026**, antes de que empiecen sus prácticas. Debe aprender y practicar directamente con **.NET 10 LTS** (soportado hasta noviembre de 2028), sin dejar de entender el código .NET 8 que encontrará en proyectos existentes. Viniendo de Spring AI, casi todo tiene equivalente directo: ChatClient ↔ IChatClient, EmbeddingModel ↔ IEmbeddingGenerator, VectorStore ↔ VectorStoreCollection. Las diferencias principales están en los "advisors" RAG, que en .NET se implementan a mano o con context providers de Agent Framework.

---

## Key Findings

1. **El ecosistema .NET de IA se ha consolidado en capas.**
   - Abajo están las abstracciones estables `Microsoft.Extensions.AI` (MEAI) y `Microsoft.Extensions.VectorData` (MEVD).
   - Encima está Microsoft Agent Framework (MAF), que construye directamente sobre `IChatClient`.
   - Semantic Kernel (SK) pasa a mantenimiento. Microsoft lo describe como "Semantic Kernel v1.x" frente a "MAF = SK v2.0", y seguirá corrigiendo bugs críticos y de seguridad durante al menos un año tras la GA de MAF.
2. **Los conectores vectoriales se han renombrado y han salido de SK.** Los paquetes `Microsoft.SemanticKernel.Connectors.PgVector`, `.Qdrant` y similares están deprecados. NuGet indica que se han renombrado a `CommunityToolkit.VectorData.*` porque "no tienen dependencia de Semantic Kernel" y que "el nuevo paquete es v1 estable". Por ejemplo, `CommunityToolkit.VectorData.Qdrant` 1.0.0 se publicó el 22 de julio de 2026. Muchos tutoriales de 2024-2025 usan los nombres antiguos.
3. **La ingesta todavía está en preview.** `Microsoft.Extensions.DataIngestion` aporta lectores, chunkers (incluido `SemanticSimilarityChunker`), procesadores y un `IngestionPipeline<T>`, pero sigue publicándose como `10.x-preview`. La plantilla `dotnet new aichatweb` también sigue en preview (10.10.0-preview.3, 9 de septiembre de 2026).
4. **MySQL no es una opción vectorial real fuera de Oracle Cloud.** El tipo `VECTOR` existe en todas las ediciones de MySQL 9.x. Sin embargo, la documentación oficial de MySQL 9.7 dice que `DISTANCE()` "solo está disponible para usuarios de MySQL HeatWave en OCI y MySQL AI; no se incluye en las distribuciones Commercial o Community". Por eso, si la consultora usa MySQL, lo sensato es poner los vectores en PostgreSQL o en MariaDB 11.8.
5. **La fecha de soporte de .NET cambia el plan de estudio.** .NET 8 (LTS) y .NET 9 (STS) terminan soporte el mismo día, el 10 de noviembre de 2026. En el caso de .NET 9 es porque el .NET Blog amplió el soporte de las versiones STS a 24 meses "starting with .NET 9"; antes su fin estaba previsto para el 12 de mayo de 2026. .NET 10 es LTS hasta el 14 de noviembre de 2028. Lo más probable es que en febrero de 2027 la consultora esté migrando o ya trabaje en .NET 10.

---

## 1. Arquitectura de un RAG

**Ingesta.** Es el proceso ETL que lleva tus documentos (PDF, Word, HTML, Markdown, filas de PostgreSQL/MySQL, tickets…) a un formato de texto normalizado con metadatos: origen, fecha, permisos, idioma. En .NET, la pieza oficial es `Microsoft.Extensions.DataIngestion` (preview). Define `IngestionDocument` y lectores como `MarkdownReader` o el lector basado en MarkItDown, además de procesadores que pueden enriquecer el contenido con IA (resúmenes, clasificación) antes de trocearlo. Buenas prácticas:
- Conservar la estructura del documento (títulos, tablas, listas), porque luego mejora el troceado.
- Guardar metadatos filtrables. El más importante es el identificador de tenant/permiso, para no filtrar datos entre clientes.
- Hacer la ingesta incremental: detectar cambios mediante hash o fecha para no reindexar todo. La plantilla `aichatweb` ya compara la carpeta de origen con el almacén y solo actualiza lo que ha cambiado.

**Troceado (chunking).** Los modelos de embeddings y la ventana de contexto del LLM funcionan mejor con fragmentos acotados, así que cada documento se divide en "chunks". Hay tres estrategias habituales:
- Por tamaño fijo en tokens con solapamiento. Es sencillo y robusto, y en .NET se implementa con `Microsoft.ML.Tokenizers`.
- Por estructura: secciones y encabezados de Markdown/HTML.
- Semántica: `SemanticSimilarityChunker` de DataIngestion corta donde cae la similitud coseno entre los embeddings de elementos consecutivos.

En la práctica suele funcionar bien empezar con chunks estructurales de unos cientos de tokens con algo de solapamiento. Conviene añadir a cada chunk el título del documento o de la sección (contextualización) y medir con un conjunto de evaluación antes de afinar. No existe un tamaño óptimo universal: depende del corpus y hay que medirlo.

**Embeddings.** Cada chunk se convierte en un vector numérico con un modelo de embeddings, por ejemplo `text-embedding-3-small` (1536 dimensiones) en Azure OpenAI/OpenAI, modelos de Amazon Bedrock o un modelo local en Ollama. En .NET, la abstracción es `IEmbeddingGenerator<string, Embedding<float>>` de MEAI, así que puedes usar Ollama en desarrollo y Azure OpenAI o Bedrock en producción sin cambiar la lógica. Reglas clave:
- Usar **el mismo modelo** para indexar y para consultar. Si cambias de modelo, tienes que reindexar.
- Fijar la dimensión en el esquema de la base de datos.
- Tener en cuenta los límites de los índices. El HNSW de pgvector está limitado a 2.000 dimensiones; DiskANN en Azure admite hasta 16.000 con cuantización de producto.
- Generar los embeddings en lotes para reducir coste y latencia.

**Almacén vectorial.** Guarda el texto del chunk, sus metadatos y su vector, e indexa este último para buscar vecinos aproximados (ANN) con HNSW, IVFFlat o DiskANN. En .NET, la abstracción es `VectorStore`/`VectorStoreCollection<TKey,TRecord>` de MEVD. Defines un POCO con atributos (clave, datos filtrables, vector) y el conector correspondiente se encarga del mapeo. Si se configura un `EmbeddingGenerator`, el propio almacén genera los embeddings al hacer upsert o búsqueda. Buenas prácticas:
- Elegir la métrica que recomienda el modelo, normalmente coseno.
- Crear el índice ANN desde el principio: sin índice, pgvector hace un escaneo secuencial.
- Marcar como filtrables los campos de permisos y de tipo de documento.

**Recuperación.** La pregunta del usuario se convierte en un embedding y se buscan los k chunks más similares, aplicando filtros de metadatos (LINQ en MEVD). La práctica actual es combinar varias técnicas:
- **Búsqueda híbrida**, que une vector y palabras clave (BM25/full-text). MEVD la soporta, igual que Azure AI Search y OpenSearch. Es importante con códigos de producto, nombres propios o siglas, que los embeddings recuperan mal.
- **Reranking**: se reordenan los top-N con un modelo cross-encoder o con el semantic ranker de Azure AI Search.
- **Transformación de la consulta**: reescribir la pregunta o expandirla con el historial de la conversación. Es lo mismo que hace `RewriteQueryTransformer` en Spring AI.
- Un **umbral de similitud** para no inyectar ruido.

**Generación.** Los chunks recuperados se insertan en el prompt como contexto, junto con instrucciones del sistema ("responde solo con el contexto; si no está, dilo; cita las fuentes"), y se llama al LLM mediante `IChatClient`. MEAI añade middleware componible: `UseFunctionInvocation()` para tool calling, `UseDistributedCache()`, `UseLogging()`, `UseOpenTelemetry()` y salida estructurada con `GetResponseAsync<T>()`, útil para devolver la respuesta junto con la lista de citas. En producción, además:
- Mostrar las citas en la interfaz.
- Hacer streaming de la respuesta.
- Limitar tokens y coste.
- Registrar trazas: qué chunks se usaron para cada respuesta.
- Evaluar de forma continua la *groundedness* (si la respuesta se apoya en el contexto) y la relevancia con `Microsoft.Extensions.AI.Evaluation`.

---

## 2. Librerías .NET: comparativa y estado 2025-2026

| Librería / paquete | Madurez (sept. 2026) | Casos de uso | ¿Activa/mantenida? Notas de estado | .NET mínimo |
|---|---|---|---|---|
| **Microsoft.Extensions.AI** (+ `.Abstractions`) | **GA** (desde 21-may-2025; v10.10.0) | Abstracción `IChatClient`, `IEmbeddingGenerator`, `IImageGenerator`; middleware (cache, logging, OpenTelemetry, function invocation); salida estructurada | **Muy activa**. Es la capa base sobre la que migran SK y MAF. SK sustituyó su `ITextEmbeddingGenerationService` por `IEmbeddingGenerator` de MEAI | Funciona en **.NET 8** |
| **Microsoft.Extensions.VectorData.Abstractions** | **GA** (desde may-2025; v10.10.0) | Abstracción `VectorStore`/`VectorStoreCollection`; CRUD, filtros LINQ, **búsqueda híbrida**, generación automática de embeddings | **Activa**. Surgió dentro de SK y ahora vive en dotnet/extensions | net8.0, net10.0, netstandard2.0 |
| **CommunityToolkit.VectorData.*** (PgVector, Qdrant, AzureAISearch, AzureCosmosDB, AzureDocumentDB, SqlServer, SqliteVec, Redis, Weaviate, InMemory) | **v1 estable** (p. ej. Qdrant 1.0.0 del 22-jul-2026; AzureAISearch 1.0.0) | Implementaciones concretas de MEVD | **Activos** (CommunityToolkit/AI, .NET Foundation). **Sustituyen** a `Microsoft.SemanticKernel.Connectors.*`, que están **deprecados**. `CosmosNoSql` pasó a llamarse `AzureCosmosDB` | net8.0, net10.0 (sin build específica net9; .NET 9 usa la de net8) |
| **Microsoft.Extensions.DataIngestion** (+ `.Abstractions`, `.Markdig`, `.MarkItDown`) | **Preview** (10.x-preview) | Lectura de documentos, enriquecimiento, chunking (token, semántico), escritura en vector store | **Activa**, API aún sujeta a cambios; conviene fijar versiones | net8.0 |
| **Microsoft.Extensions.AI.Evaluation** (+ `.Quality`, `.Safety`, `.Reporting`, `.Console`) | GA en la familia MEAI (evaluadores con LLM como juez) | Evaluar relevancia, completitud, coherencia y *groundedness* (1-5) en tests; informes HTML | **Activa**. Los evaluadores de *Safety* dependen del servicio de evaluación de Microsoft Foundry | .NET 8+ |
| **Microsoft Agent Framework** (`Microsoft.Agents.AI`) | **GA 1.0** (public preview oct-2025 → RC 19-feb-2026 → 1.0 el 2-abr-2026, según el blog oficial de MAF) | Agentes, herramientas, MCP/A2A, context providers, workflows multiagente con checkpointing y *human-in-the-loop* | **Muy activa**. **Sucesor oficial de Semantic Kernel y AutoGen**. Algunos paquetes (p. ej. hosting en Foundry) siguen en preview | Construido sobre MEAI; comprobar TFM por paquete |
| **Semantic Kernel** (`Microsoft.SemanticKernel`) | **GA v1.x, en modo mantenimiento** | Plugins/funciones, prompt templates, filtros, orquestación existente | **Mantenida**: bugs críticos y seguridad durante al menos 1 año tras la GA de MAF (es decir, como mínimo hasta ~abril de 2027). Las novedades van a MAF. Su funcionalidad Vector Store figura como "preview" en su documentación | .NET 8+ |
| **Kernel Memory** (`Microsoft.KernelMemory.*`) | **Obsoleta / archivada** | Servicio/biblioteca RAG completo (ingesta, citas) | **No mantenida**. El repo se declara "archived research project… no support is provided". Los paquetes NuGet están marcados como "deprecated… legacy and no longer maintained" (última versión 0.98.250508.3, mayo de 2025). Sus ideas pasaron a MEVD, DataIngestion y MAF | — |
| **OpenAI** (SDK oficial .NET) | **GA** (2.x) | Cliente directo de OpenAI y del endpoint v1 de Azure OpenAI; `.AsIChatClient()` vía `Microsoft.Extensions.AI.OpenAI` | **Activo**. Con la API v1 de Azure OpenAI (GA desde ago-2025) se puede usar el cliente OpenAI directamente contra Azure | .NET 8+ |
| **Azure.AI.OpenAI** | **GA** (2.1.0) + betas | "Compañero" del SDK OpenAI: autenticación con Entra ID y extensiones propias de Azure | **Activo**. Las versiones GA solo incluyen APIs estables; las funciones preview requieren prerelease. El antiguo **Azure AI Inference SDK** (beta) está **retirado/deprecado** en favor del SDK OpenAI + API v1 | .NET 8+ |
| **AWSSDK.BedrockRuntime** + **AWSSDK.Extensions.Bedrock.MEAI** | **GA** (4.0.x) | `IChatClient`/`IEmbeddingGenerator` sobre Amazon Bedrock (Claude, Llama, Mistral, Titan…) | **Activo** (mantenido por AWS; MAF lo documenta como proveedor) | netstandard2.0 (válido en .NET 8/10) |
| **AWSSDK.BedrockAgentRuntime** | GA | Consumir Bedrock Knowledge Bases (`Retrieve`, `RetrieveAndGenerate`) desde .NET | Activo; es el SDK clásico de AWS, **sin** integración MEVD | .NET 8+ |
| **Plantilla "AI Chat Web App"** (`Microsoft.Extensions.AI.Templates`, `dotnet new aichatweb`) | **Preview** (10.10.0-preview.3, 9-sep-2026) | Arranque de un chat RAG en Blazor con citas, ingesta de PDFs, opción Aspire; proveedores GitHub Models/Azure OpenAI/OpenAI/Ollama; vector store local, Qdrant o Azure AI Search | **Activa**, pero las opciones cambian entre versiones (hay issues por parámetros renombrados). La guía de Learn aún pide SDK .NET 9 como mínimo; las versiones actuales se prueban con SDK 10 | SDK **.NET 9+** (recomendado 10) |
| **LangChain .NET** (comunitario) | Experimental/comunitario | Portar patrones de LangChain | **Sin respaldo de Microsoft** y con un ecosistema más pequeño. No se ha podido verificar su actividad en 2026; no se recomienda para una consultora | — |

**Cómo encaja todo:** MEAI y MEVD son el "JDBC/Spring Data" de la IA en .NET, contratos estables que todo el mundo implementa. Los conectores CommunityToolkit son los "drivers". DataIngestion es el "Spring Batch/ETL" (aún en preview). MAF es la capa de agentes. SK sigue funcionando en código heredado, pero no conviene empezar proyectos nuevos con él.

### Qué requiere .NET 8, 9 o 10

| Funcionalidad | .NET 8 LTS | .NET 9 | .NET 10 LTS |
|---|---|---|---|
| MEAI, MEVD, DataIngestion, CommunityToolkit.VectorData, Bedrock MEAI | ✅ (TFM net8.0/netstandard2.0) | ✅ (usa build net8) | ✅ (TFM net10.0 en varios paquetes) |
| `Pgvector.EntityFrameworkCore` última versión (EF Core 9 y 10) | ✅ con EF Core 9 (EF Core 9 corre en .NET 8); con EF Core 8, usar 0.2.2 | ✅ | ✅; **EF Core 10 exige .NET 10** |
| Plantilla `aichatweb` | ❌ (necesita SDK 9+) | ✅ mínimo | ✅ recomendado |
| Apps de un solo fichero con `#:package` (útil para prototipos rápidos) | ❌ | ❌ | ✅ **solo .NET 10** |
| Soporte oficial | **Termina 10-nov-2026** | **Termina 10-nov-2026** | Hasta 14-nov-2028 |

---

## 3. Base de datos vectorial: opciones y conectores .NET

### Opciones "en tu propia base de datos" (PostgreSQL/MySQL)

| Opción | Estado | Notas técnicas | Conector .NET | Estado del conector |
|---|---|---|---|---|
| **PostgreSQL + pgvector** | Estable, estándar de facto | HNSW e IVFFlat; operadores `<=>` (coseno), `<->` (L2), `<#>` (producto interno); índice HNSW hasta 2.000 dimensiones | (a) `CommunityToolkit.VectorData.PgVector` (MEVD); (b) `Pgvector` + `Npgsql` (ADO.NET/Dapper); (c) `Pgvector.EntityFrameworkCore` (`UseVector()`, `HasPostgresExtension("vector")`) | (a) **v1 estable**; (b)/(c) estables, mantenidos por el autor de pgvector |
| **pgvectorscale** (Timescale) | Extensión open source, más reciente | Añade StreamingDiskANN y cuantización sobre pgvector, para volúmenes grandes | Usa los mismos tipos de pgvector, así que vale Npgsql + SQL | Sin conector MEVD específico; **comprueba si tu proveedor gestionado lo permite** (no aparece en la documentación de Azure/AWS consultada) |
| **MySQL 9.x Community/Commercial** | Tipo `VECTOR` disponible | **Sin `DISTANCE()` ni índice vectorial** fuera de HeatWave/MySQL AI; habría que calcular distancias en la aplicación | MySqlConnector/Connector/NET (solo almacenamiento) | **No recomendable para RAG** |
| **MySQL HeatWave (OCI)** | GA | `DISTANCE()` con COSINE/DOT/EUCLIDEAN, vector store y pipeline GenAI integrados | Sin conector MEVD | Solo tiene sentido si la consultora trabaja en Oracle Cloud |
| **MariaDB Vector** | **GA en 11.8 LTS** (preview en 11.7) | `VECTOR(N)`, `VECTOR INDEX` (HNSW modificado), `VEC_DISTANCE_COSINE/EUCLIDEAN`, hasta 16.383 dimensiones, ACID; disponible en Amazon RDS for MariaDB | MySqlConnector + SQL (`VEC_FromText`) | **Sin conector MEVD oficial**; Spring AI sí tiene integración |

### Servicios gestionados en Azure

| Servicio | Estado | Para qué | Conector .NET |
|---|---|---|---|
| **Azure Database for PostgreSQL (Flexible Server) + pgvector + DiskANN** | pgvector GA; **DiskANN (`pg_diskann`) GA**; cuantización de producto en preview; soporte en PostgreSQL 18 (abr-2026) | La opción más natural si la consultora ya usa PostgreSQL. Según Abe Omorogbe (Senior PM de Microsoft), DiskANN ofrece "up to 10x faster speed, 4x lower costs and up to 96x lower memory footprint" frente al HNSW de pgvector, medido en una prueba interna con 35 millones de vectores de 768 dimensiones (**dato del fabricante**). Admite hasta 16.000 dimensiones con PQ | `CommunityToolkit.VectorData.PgVector` (v1) o Npgsql + Pgvector/EF Core |
| **Azure AI Search** | GA | Motor de búsqueda gestionado con búsqueda híbrida, semantic ranker (reranking) y *integrated vectorization*; es la opción "Azure" de la plantilla `aichatweb` | `CommunityToolkit.VectorData.AzureAISearch` **1.0.0** |
| **Azure Cosmos DB for NoSQL (vector search con DiskANN)** | GA | Datos operativos NoSQL, memoria de chat y vectores en el mismo sitio; escala global | `CommunityToolkit.VectorData.AzureCosmosDB` (antes `CosmosNoSql`) |
| **Azure DocumentDB / Cosmos DB for MongoDB vCore** | GA | Si el equipo trabaja con MongoDB | `CommunityToolkit.VectorData.AzureDocumentDB` |
| **Azure SQL / SQL Server** (tipo vector) | Disponible | Si el cliente está en SQL Server | `CommunityToolkit.VectorData.SqlServer` |

### Servicios gestionados en AWS

| Servicio | Estado | Para qué | Conector .NET |
|---|---|---|---|
| **Amazon Aurora / RDS for PostgreSQL + pgvector** | GA | El equivalente directo a Azure PostgreSQL; mismo código .NET | `CommunityToolkit.VectorData.PgVector` o Npgsql + Pgvector (el mismo que en local) |
| **Amazon OpenSearch Service / Serverless** | GA | Búsqueda híbrida (k-NN + BM25) a escala; almacén por defecto de Bedrock Knowledge Bases; puede delegar el almacenamiento vectorial en S3 Vectors | **Sin conector MEVD oficial**; usar el cliente OpenSearch para .NET o consumirlo vía Bedrock KB |
| **Amazon Bedrock Knowledge Bases** | GA (desde nov-2023) | **RAG totalmente gestionado**: ingiere desde S3, trocea, genera embeddings y ofrece las APIs `Retrieve` y `RetrieveAndGenerate`; admite OpenSearch Serverless, S3 Vectors, Aurora, Pinecone, Redis… | `AWSSDK.BedrockAgentRuntime` (SDK clásico, sin MEVD) |
| **Amazon S3 Vectors** | **GA (2-dic-2025, AWS What's New)**: hasta 2.000 millones de vectores por índice, 40 veces más que en la preview; 14 regiones en el lanzamiento, ampliadas en 17 más el 31-mar-2026 (31 en total) | Almacenamiento vectorial barato para grandes volúmenes. Según AWS, las consultas poco frecuentes responden en menos de un segundo y las frecuentes "around 100 milliseconds or less". Integración GA con Bedrock KB y OpenSearch | **Sin conector MEVD**; se usa vía Bedrock KB o el SDK de AWS |
| **Amazon RDS for MariaDB 11.8** | GA | MariaDB Vector gestionado | MySqlConnector + SQL |

**Recomendación:** para una consultora que ya domina PostgreSQL y despliega en ambas nubes, **pgvector es la opción más portable**. El mismo código con `CommunityToolkit.VectorData.PgVector` corre en Docker local, en Azure Database for PostgreSQL y en Aurora/RDS. Los servicios gestionados (Azure AI Search, Bedrock KB) compensan cuando se necesitan híbrido y reranking "llave en mano" o cuando el volumen es muy grande, a cambio de más dependencia del proveedor y de perder la abstracción MEVD en el caso de AWS.

---

## 4. Equivalencias Java (Spring AI) ↔ .NET

Nota de versiones: Spring AI 1.0 salió GA el 20 de mayo de 2025. **Spring AI 2.0 salió GA en junio de 2026** sobre Spring Boot 4 / Spring Framework 7, requiere Java 21 y no carga en Spring Boot 3.x. Las abstracciones siguientes existen en 1.x y se mantienen en 2.0.

| Concepto | Spring AI (1.x / 2.0) | .NET | Comentario |
|---|---|---|---|
| Cliente de chat de alto nivel | `ChatClient` (API fluida `.prompt().user().call()`) | `IChatClient` (MEAI) + `ChatClientBuilder` | `IChatClient` hace a la vez de `ChatModel` y de `ChatClient`; la fluidez se consigue con el builder y el middleware |
| Modelo de chat de bajo nivel | `ChatModel` | Implementaciones de `IChatClient` (`OpenAIClient.GetChatClient().AsIChatClient()`, `AsChatClient()` de Bedrock, Ollama) | Mismo concepto de "proveedor" intercambiable |
| Embeddings | `EmbeddingModel` | `IEmbeddingGenerator<string, Embedding<float>>` | Equivalencia 1:1 |
| Almacén vectorial | `VectorStore` (extiende `DocumentWriter` y `VectorStoreRetriever`; `similaritySearch(SearchRequest)`) | `VectorStore` / `VectorStoreCollection<TKey,TRecord>` (MEVD): `UpsertAsync`, `SearchAsync`, `HybridSearchAsync` | En .NET el registro es **tu propio POCO tipado** con atributos; en Spring es un `Document` genérico con metadatos |
| Implementación pgvector | `PgVectorStore` (starter `spring-ai-starter-vector-store-pgvector`) | `CommunityToolkit.VectorData.PgVector` o `Pgvector.EntityFrameworkCore` | La tabla es parecida; en .NET puedes mapearla también con EF Core |
| Filtros de metadatos | `Filter.Expression` / cadena tipo SQL | Expresiones **LINQ** (`Filter = r => r.TenantId == "x"`) | LINQ ofrece tipado fuerte |
| RAG simple | `QuestionAnswerAdvisor` | Sin clase equivalente: se hace con unas líneas (buscar → componer prompt → `GetResponseAsync`), con un middleware de `IChatClient`, o con un *context provider* / `TextSearchProvider` en Agent Framework | Diferencia importante: .NET no trae un "advisor RAG" en MEAI |
| RAG modular | `RetrievalAugmentationAdvisor` + `RewriteQueryTransformer`, `VectorStoreDocumentRetriever`, `DocumentPostProcessor` | Pipeline propio: reescritura con `IChatClient`, búsqueda híbrida con MEVD, reranking propio o semantic ranker de Azure AI Search | Los conceptos se corresponden 1:1, pero en .NET los compones tú |
| Cadena de advisors / interceptores | `Advisor` (`CallAdvisor`, `StreamAdvisor`) | Middleware de `IChatClient` (`.Use(...)`, `DelegatingChatClient`) y middleware de MAF | Mismo patrón "chain of responsibility" |
| Tool calling | `@Tool` + `ToolCallingAdvisor` / `.tools(...)` | `AIFunctionFactory.Create(metodo)` + `.UseFunctionInvocation()`; en SK, `[KernelFunction]` y plugins; en MAF, *function tools* | `[Description]` en .NET cumple el papel de la descripción de `@Tool` |
| Memoria de chat | `ChatMemory`, `MessageWindowChatMemory`, `ChatMemoryRepository`, `MessageChatMemoryAdvisor` | Lista de `ChatMessage` gestionada por ti, o sesiones/threads con memoria en Agent Framework | En MEAI puro no hay repositorio de memoria; en MAF sí |
| ETL / ingesta | `DocumentReader` (PDF, Tika, JSON…), `DocumentTransformer` (`TokenTextSplitter`), `DocumentWriter` | `Microsoft.Extensions.DataIngestion`: readers (Markdig, MarkItDown), `IngestionChunker<T>`, `IngestionChunkProcessor<T>`, `IngestionChunkWriter<T>`, `IngestionPipeline<T>` | Correspondencia casi directa; en .NET aún es **preview** |
| Salida estructurada | `.entity(MiClase.class)` / `BeanOutputConverter` | `GetResponseAsync<T>()` | Equivalente |
| Evaluación | `Evaluator` (`RelevancyEvaluator`, `FactCheckingEvaluator`) | `Microsoft.Extensions.AI.Evaluation.Quality` (`RelevanceEvaluator`, `GroundednessEvaluator`, `CompletenessEvaluator`…) | .NET añade informes y caché de respuestas |
| Observabilidad | Micrometer / Observation API | `UseOpenTelemetry()` + `ILogger` | Aspire muestra las trazas en su dashboard |
| Inyección de dependencias | Spring IoC (`@Bean`, `@Autowired`, constructor injection) | `Microsoft.Extensions.DependencyInjection` (`builder.Services.AddSingleton<IChatClient>(...)`, keyed services) | Solo inyección por constructor; no hay escaneo de componentes por anotaciones |
| Autoconfiguración | Spring Boot starters + `application.yml` | `Program.cs` + `appsettings.json` + extensiones `AddXxx()`; integraciones de Aspire | En .NET se configura en código |
| Agentes / MCP | Spring AI agents, MCP client/server starters | Microsoft Agent Framework (MCP, A2A, workflows) | MAF va algo más lejos en multiagente |
| Proyecto de arranque | Spring Initializr | `dotnet new aichatweb` (preview), `dotnet new webapi` | — |

---

## 5. Ruta de aprendizaje para Igor (octubre 2026 → febrero 2027)

Hay unas 18-20 semanas por delante. Instala el **SDK de .NET 10** y trabaja siempre con `net10.0`, aunque debes saber leer proyectos `net8.0`, porque la consultora puede tener código heredado en plena migración. Cada fase termina con un mini-proyecto en GitHub, que además sirve de portfolio para enseñar el primer día.

**Fase 1 — C# para javeros (semanas 1-3).**
- Temas:
  - Tipos por valor y por referencia (`struct` frente a `class`), `record`, propiedades, nullable reference types (`string?`, más estrictos que `@Nullable`).
  - `async/await` y `Task` (el equivalente a `CompletableFuture`, pero integrado en el lenguaje), `IAsyncEnumerable` y `await foreach`, que usarás para el streaming de LLMs.
  - LINQ (tu nuevo Stream API), genéricos reificados, extension methods, pattern matching, `using`/`IDisposable` (≈ try-with-resources).
  - Herramientas: `dotnet` CLI, `.csproj` (≈ `pom.xml`), NuGet (≈ Maven Central), xUnit (≈ JUnit).
- *Mini-proyecto:* una CLI que lea una carpeta de Markdown, cuente tokens con `Microsoft.ML.Tokenizers` y trocee los ficheros en chunks de N tokens con solapamiento, con tests en xUnit.

**Fase 2 — ASP.NET Core (semanas 4-6).**
- Temas:
  - Minimal APIs y controladores (≈ `@RestController`).
  - `Program.cs` como punto de configuración, DI con `AddSingleton/AddScoped/AddTransient` (≈ scopes de Spring).
  - Options pattern + `appsettings.json` (≈ `@ConfigurationProperties`), middleware (≈ filtros de Servlet), `ILogger`, validación, OpenAPI y User Secrets para las API keys.
- *Mini-proyecto:* una API REST de "gestor de documentos" (subida, listado, borrado) con tests de integración (`WebApplicationFactory` ≈ `@SpringBootTest`).

**Fase 3 — EF Core + PostgreSQL (semanas 7-8).**
- Temas: `DbContext` (≈ `EntityManager`/repositorios de Spring Data), migraciones (`dotnet ef migrations add` ≈ Flyway/Liquibase), relaciones, consultas LINQ, `AsNoTracking`, Npgsql y PostgreSQL en Docker. Echa también un vistazo a Dapper (≈ JdbcTemplate).
- *Mini-proyecto:* guardar los documentos de la fase 2 en PostgreSQL con EF Core y migraciones.

**Fase 4 — Microsoft.Extensions.AI (semanas 9-10).**
- Temas:
  - `IChatClient` con Ollama en local (gratis) y con GitHub Models, Azure OpenAI u OpenAI.
  - Streaming, `GetResponseAsync<T>` para salida estructurada.
  - Tool calling con `AIFunctionFactory` + `UseFunctionInvocation()`.
  - Middleware `UseDistributedCache`/`UseOpenTelemetry`, y registro en DI con cambio de proveedor por configuración.
- *Mini-proyecto:* un endpoint `/chat` con streaming que pueda "consultar el estado de un pedido" mediante una herramienta que lee de PostgreSQL.

**Fase 5 — Embeddings + pgvector (semanas 11-12).**
- Temas: `IEmbeddingGenerator`, similitud coseno "a mano" para entenderla, imagen Docker `pgvector/pgvector`, `CREATE EXTENSION vector`, índice HNSW. Implementa lo mismo de dos maneras:
  - (a) con `Pgvector.EntityFrameworkCore` (`UseVector()`, `CosineDistance`);
  - (b) con `CommunityToolkit.VectorData.PgVector` (POCO con atributos + `SearchAsync`).
  - Compara ambas y mide con y sin índice.
- *Mini-proyecto:* un buscador semántico de preguntas frecuentes.

**Fase 6 — Mini-RAG de principio a fin (semanas 13-15).**
- Construye **"AsturDocs RAG"**: preguntas y respuestas sobre un corpus real y público, por ejemplo normativa del Principado, documentación técnica o PDFs de un organismo. El pipeline:
  - Ingesta con `Microsoft.Extensions.DataIngestion` (fija la versión preview que uses) o con tu propio chunker de la fase 1.
  - Metadatos (fuente, sección, `tenantId`).
  - Búsqueda híbrida (vector + full-text de PostgreSQL) y umbral de similitud.
  - Prompt con citas y salida estructurada `{respuesta, fuentes[]}`, streaming y una interfaz sencilla en Blazor.
- Después genera un proyecto con `dotnet new aichatweb` (preview) y **compara su arquitectura con la tuya**. Es la forma más rápida de ver cómo lo plantea Microsoft.
- *Extra:* reescribir la consulta con el historial de la conversación (equivalente a `RewriteQueryTransformer`) y un reranking simple.

**Fase 7 — Evaluación del RAG (semana 16).**
- Crea un "golden set" de 30-50 preguntas con su respuesta esperada y los documentos que deberían recuperarse. Mide:
  - La recuperación con hit rate y MRR, calculados en C# sin LLM.
  - La generación con `Microsoft.Extensions.AI.Evaluation.Quality` (`RelevanceEvaluator`, `GroundednessEvaluator`, `CompletenessEvaluator`) dentro de xUnit, y genera el informe con la herramienta de consola.
- Úsalo como "quality gate": cambia el tamaño de chunk o activa el modo híbrido y compara los resultados. Ten presente que los evaluadores que usan un LLM como juez son probabilísticos.

**Fase 8 — Aspire + despliegue en Azure y AWS (semanas 17-19).**
- **Aspire**, la tecnología antes conocida como ".NET Aspire", que usa la plantilla aichatweb. Orquesta en local la API, PostgreSQL y Ollama con un dashboard de trazas OpenTelemetry. Es lo más parecido a Spring Boot + Docker Compose + Micrometer juntos.
- **Azure:** contenedor en Azure Container Apps o App Service, Azure Database for PostgreSQL Flexible Server con `vector` (y prueba `pg_diskann`), Azure OpenAI con **Managed Identity/Entra ID** en lugar de API keys y secretos en Key Vault. `azd` (Azure Developer CLI) simplifica el despliegue.
- **AWS:** el mismo contenedor en ECS Fargate o App Runner, Aurora/RDS PostgreSQL con pgvector y Bedrock mediante `AWSSDK.Extensions.Bedrock.MEAI`: cambias el `IChatClient` en `Program.cs` y el resto del código no se toca. *Opcional:* crear una Bedrock Knowledge Base con S3 Vectors y llamarla con `RetrieveAndGenerate` para comparar el enfoque gestionado con el tuyo.
- Añade CI con GitHub Actions: build, tests y evaluación.

**Fase 9 — Agentes (semana 20, opcional).** Convierte tu RAG en un agente con Microsoft Agent Framework 1.0: la búsqueda como herramienta y un segundo agente que resuma. Aprende lo básico de MCP. Lee la guía oficial de migración de SK a MAF, porque es probable que la consultora tenga código SK que migrar.

**Consejos prácticos:**
- Al buscar tutoriales, **descarta los que usen `Microsoft.SemanticKernel.Connectors.*`, `ISemanticTextMemory`/`MemoryStore` (el API de memoria antiguo de SK) o Kernel Memory**, porque te enseñarán APIs deprecadas.
- Pregunta a tu tutor en la consultora qué versión de .NET y qué librerías usa su plataforma RAG. Si está sobre Semantic Kernel o Kernel Memory, habrá trabajo de migración, y conocer MEAI, MEVD y MAF te hará valioso desde el primer día.

---

## Caveats

- **Estado cambiante:** varias piezas avanzan casi mes a mes (MEAI 10.x, DataIngestion preview, la plantilla aichatweb preview, paquetes de hosting de MAF en preview). Comprueba la página de NuGet antes de fijar versiones.
- **Conectores CommunityToolkit:** está verificado el 1.0.0 estable de Qdrant (22-jul-2026) y AzureAISearch, y NuGet describe PgVector como "v1 estable". La versión exacta del resto (SqlServer, SqliteVec, Redis, Weaviate, AzureCosmosDB…) no se ha comprobado paquete a paquete. No se ha encontrado un anuncio oficial del cambio de nombre en devblogs; la fuente son las propias páginas de NuGet y Microsoft Learn.
- **Uso de DataIngestion en la plantilla:** que `aichatweb` use `Microsoft.Extensions.DataIngestion` se apoya en fuentes indirectas (post de devblogs y análisis del repo), no en la inspección del `.csproj` generado.
- **Datos de rendimiento:** son **afirmaciones de los propios fabricantes**; valídalas con tus datos.
  - Microsoft dice que DiskANN es "up to 10x faster" que el HNSW de pgvector, según una prueba interna con 35 millones de vectores.
  - AWS dice que S3 Vectors "reduces the total costs to upload, store, and query vectors by up to 90%" frente a bases de datos vectoriales especializadas.
  - MariaDB publica benchmarks propios frente a pgvector.
- **Fecha de GA de MAF:** el blog oficial de Microsoft Agent Framework dice "MAF reached 1.0 GA on April 2, 2026", mientras que Visual Studio Magazine sitúa el anuncio el 3 de abril de 2026.
- **LangChain .NET:** no se ha podido verificar su actividad en 2026; trátalo como experimental.
- **Spring AI:** la petición mencionaba 1.x, pero la versión actual es 2.0 (GA en junio de 2026, sobre Boot 4). Las equivalencias de la tabla valen para ambas; la migración de 1.x a 2.0 implica Spring Boot 4 y Jackson 3.