using Microsoft.AspNetCore.Mvc;
using MarinaApi.Dtos;
using MarinaApi.Services;

namespace MarinaApi.Controllers;

/// <summary>
/// Equivalente a BarcoController.java (Capítulo 13). [ApiController] equivale
/// a @RestController: serializa a JSON automáticamente y activa validación
/// automática del modelo (ver BarcoRequestDto). [Route] equivale a
/// @RequestMapping. La documentación Swagger se genera SOLA a partir de los
/// tipos y atributos [ProducesResponseType] — en Java hacía falta añadir
/// @Tag/@Operation/@ApiResponse manualmente en cada método (Capítulo 15);
/// aquí Swashbuckle infiere casi todo del propio código.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BarcosController : ControllerBase
{
    private readonly IBarcoService _barcoService;

    public BarcosController(IBarcoService barcoService)
    {
        _barcoService = barcoService;
    }

    /// <summary>Lista todos los barcos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BarcoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BarcoDto>>> GetAll(CancellationToken ct) =>
        Ok(await _barcoService.FindAllAsync(ct));

    /// <summary>Busca un barco por su Id.</summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BarcoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarcoDto>> GetById(long id, CancellationToken ct) =>
        Ok(await _barcoService.FindByIdAsync(id, ct));
    // Nota: NO hay "if (barco == null) return NotFound()" como en Java (Cap. 13).
    // Si no existe, el servicio lanza NotFoundException y el middleware global
    // (Middleware/ExceptionHandlingMiddleware.cs) la convierte en 404 automáticamente.

    /// <summary>Crea un barco nuevo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BarcoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BarcoDto>> Create([FromBody] BarcoRequestDto dto, CancellationToken ct)
    {
        var creado = await _barcoService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    /// <summary>Actualiza un barco existente.</summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(BarcoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BarcoDto>> Update(long id, [FromBody] BarcoRequestDto dto, CancellationToken ct) =>
        Ok(await _barcoService.UpdateAsync(id, dto, ct));

    /// <summary>Elimina un barco.</summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _barcoService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Filtra barcos por tipo (velero, motor...).</summary>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(List<BarcoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BarcoDto>>> GetByTipo(string tipo, CancellationToken ct) =>
        Ok(await _barcoService.FindByTipoAsync(tipo, ct));

    /// <summary>Cuenta barcos de un tipo concreto.</summary>
    [HttpGet("tipo/{tipo}/count")]
    [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
    public async Task<ActionResult<long>> CountByTipo(string tipo, CancellationToken ct) =>
        Ok(await _barcoService.CountByTipoAsync(tipo, ct));

    /// <summary>Obtiene el promedio de eslora para un tipo de barco (Cap. 8.2.5: Agregación).</summary>
    [HttpGet("tipo/{tipo}/promedio-eslora")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetPromedioEsloraByTipo(string tipo, CancellationToken ct) =>
        Ok(await _barcoService.GetPromedioEsloraByTipoAsync(tipo, ct));
}
