namespace MarinaApi.Dtos;

/// <summary>
/// Respuesta paginada genérica. Equivalente a Page&lt;T&gt; de Spring Data:
/// los elementos de la página pedida más los datos para que el cliente
/// pueda pintar la paginación (total de elementos y de páginas).
/// </summary>
/// <typeparam name="T">Tipo de cada elemento (BarcoDto, RegataDto...).</typeparam>
public record PaginaDto<T>(
    List<T> Elementos,
    int Pagina,
    int Tamanio,
    int TotalElementos
)
{
    public int TotalPaginas => (int)Math.Ceiling(TotalElementos / (double)Tamanio);
}