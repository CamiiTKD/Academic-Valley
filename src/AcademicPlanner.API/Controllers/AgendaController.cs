using AcademicPlanner.Application.Features.Agenda.DTOs;
using AcademicPlanner.Application.Features.Agenda.Queries.GetAgendaSemanal;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.API.Controllers;

/// <summary>
/// Consulta la agenda semanal de materias en cursado.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AgendaController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Devuelve la grilla semanal de horarios de las materias que se están cursando actualmente,
    /// estructurada por día de la semana y ordenada por hora de inicio.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de días con sus horarios de cursada.</returns>
    /// <response code="200">Agenda semanal obtenida correctamente.</response>
    [HttpGet("semanal")]
    [ProducesResponseType(typeof(IReadOnlyList<DiaAgendaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerAgendaSemanal(CancellationToken ct)
    {
        var resultado = await mediator.Send(new GetAgendaSemanalQuery(), ct);
        return Ok(resultado);
    }
}
