using AcademicPlanner.Application.Features.Progreso.DTOs;
using AcademicPlanner.Application.Features.Progreso.Queries.CalcularProgreso;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.API.Controllers;

/// <summary>
/// Consulta el progreso académico de la carrera.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProgresoController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Calcula y devuelve el progreso actual de la carrera.
    /// Incluye porcentaje de avance, créditos acumulados y promedios (con y sin aplazos).
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Resumen del progreso académico.</returns>
    /// <response code="200">Progreso calculado exitosamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ProgresoCarreraDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProgresoCarreraDto>> ObtenerProgreso(CancellationToken ct)
    {
        var resultado = await mediator.Send(new CalcularProgresoQuery(), ct);
        return Ok(resultado);
    }
}
