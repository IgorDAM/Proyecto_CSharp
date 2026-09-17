using Microsoft.AspNetCore.Mvc;
using MarinaApi.Dtos;
using MarinaApi.Services;

namespace MarinaApi.Controllers;

/// <summary>Equivalente a RegataController.java, más los endpoints de inscripción N:M.</summary>
[ApiController]
[Route("api/[controller]")]
public class RegatasController : ControllerBase
{
    private readonly IRegataService _regataService;

    public RegatasController(IRegataService regataService)
    {
        _regataService = regataService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RegataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RegataDto>>> GetAll(CancellationToken ct) =>
        Ok(await _regataService.FindAllAsync(ct));

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(RegataDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegataDto>> GetById(long id, CancellationToken ct) =>
        Ok(await _regataService.FindByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(RegataDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RegataDto>> Create([FromBody] RegataRequestDto dto, CancellationToken ct)
    {
        var creada = await _regataService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _regataService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("lugar/{lugar}")]
    [ProducesResponseType(typeof(List<RegataDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RegataDto>>> GetByLugar(string lugar, CancellationToken ct) =>
        Ok(await _regataService.FindByLugarAsync(lugar, ct));

    /// <summary>Inscribe un barco en una regata (relación N:M).</summary>
    [HttpPost("{regataId:long}/barcos/{barcoId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InscribirBarco(long regataId, long barcoId, CancellationToken ct)
    {
        await _regataService.InscribirBarcoAsync(regataId, barcoId, ct);
        return NoContent();
    }

    /// <summary>Retira un barco de una regata.</summary>
    [HttpDelete("{regataId:long}/barcos/{barcoId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DesinscribirBarco(long regataId, long barcoId, CancellationToken ct)
    {
        await _regataService.DesinscribirBarcoAsync(regataId, barcoId, ct);
        return NoContent();
    }

        /// <summary>Total de tripulantes inscritos en la regata, sumando los de todos sus barcos.</summary>
    [HttpGet("{id:long}/tripulantes-totales")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<int>> GetTripulantesTotales(long id, CancellationToken ct) =>
        Ok(await _regataService.ContarTripulantesTotalesAsync(id, ct));
}
