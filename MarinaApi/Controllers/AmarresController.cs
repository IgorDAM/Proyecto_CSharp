using Microsoft.AspNetCore.Mvc;
using MarinaApi.Dtos;
using MarinaApi.Services;
using System.ComponentModel.DataAnnotations;

namespace MarinaApi.Controllers;

/// <summary>Equivalente a AmarreController.java.</summary>
[ApiController]
[Route("api/[controller]")]
public class AmarresController : ControllerBase
{
    private readonly IAmarreService _amarreService;

    public AmarresController(IAmarreService amarreService)
    {
        _amarreService = amarreService;
    }

    /// <summary>
    /// Lista los amarres paginados, ordenados por Id. La página empieza en 1;
    /// el tamaño por defecto es 20 y el máximo 100.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<AmarreDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaDto<AmarreDto>>> GetAll(
        [FromQuery, Range(1, int.MaxValue)] int pagina = 1,
        [FromQuery, Range(1, 100)] int tamanio = 20,
        CancellationToken ct = default) =>
        Ok(await _amarreService.FindPageAsync(pagina, tamanio, ct));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AmarreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AmarreDto>> GetById(long id, CancellationToken ct) =>
        Ok(await _amarreService.FindByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(AmarreDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AmarreDto>> Create([FromBody] AmarreRequestDto dto, CancellationToken ct)
    {
        var creado = await _amarreService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _amarreService.DeleteAsync(id, ct);
        return NoContent();
    }

    /// <summary>Amarres sin barco asignado.</summary>
    [HttpGet("libres")]
    [ProducesResponseType(typeof(List<AmarreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AmarreDto>>> GetLibres(CancellationToken ct) =>
        Ok(await _amarreService.FindLibresAsync(ct));

    /// <summary>Amarres con suministro eléctrico.</summary>
    [HttpGet("con-electricidad")]
    [ProducesResponseType(typeof(List<AmarreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AmarreDto>>> GetConElectricidad(CancellationToken ct) =>
        Ok(await _amarreService.FindConElectricidadAsync(ct));

    [HttpPatch("{id:long}/barco")]
    [ProducesResponseType(typeof(AmarreDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AmarreDto>> AsignarBarco(long id, [FromBody] AsignarBarcoDto dto, CancellationToken ct)
    {
        var actualizado = await _amarreService.AssignBarcoAsync(id, dto, ct);
        return Ok(actualizado);
    }
}