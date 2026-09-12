using Microsoft.AspNetCore.Mvc;
using MarinaApi.Dtos;
using MarinaApi.Services;

namespace MarinaApi.Controllers;

// Esto es un atributo que indica que esta clase es un controlador de API y habilita características específicas
// para controladores de API, como la validación automática de modelos y la serialización de respuestas.
[ApiController]
[Route("api/[controller]")]
public class TripulantesController : ControllerBase
{
    private readonly ITripulanteService _tripulanteService;

    // Constructor que inyecta la dependencia del servicio de tripulantes
    public TripulantesController(ITripulanteService tripulanteService)
    {
        _tripulanteService = tripulanteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TripulanteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TripulanteDto>>> GetAll(CancellationToken ct) =>
        Ok(await _tripulanteService.FindAllAsync(ct));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(TripulanteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripulanteDto>> GetById(long id, CancellationToken ct) =>
        Ok(await _tripulanteService.FindByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(TripulanteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TripulanteDto>> Create([FromBody] TripulanteRequestDto dto, CancellationToken ct)
    {
        var creado = await _tripulanteService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(TripulanteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TripulanteDto>> Update(long id, [FromBody] TripulanteRequestDto dto, CancellationToken ct) =>
        Ok(await _tripulanteService.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _tripulanteService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("barco/{barcoId:long}")]
    [ProducesResponseType(typeof(List<TripulanteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TripulanteDto>>> GetByBarcoId(long barcoId, CancellationToken ct) =>
        Ok(await _tripulanteService.FindByBarcoIdAsync(barcoId, ct));
}