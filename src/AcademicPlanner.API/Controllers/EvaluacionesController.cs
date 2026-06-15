using AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;
using AcademicPlanner.Application.Features.Evaluaciones.DTOs;
using AcademicPlanner.Application.Features.Evaluaciones.Queries.GetEvaluaciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.API.Controllers;

/// <summary>
/// Gestión de evaluaciones asociadas a una materia.
/// </summary>
[ApiController]
[Route("api/Materias/{materiaId:guid}/evaluaciones")]
[Produces("application/json")]
public class EvaluacionesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Lista las evaluaciones de una materia.
    /// </summary>
    /// <param name="materiaId">Id de la materia.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 OK con la lista de evaluaciones.</returns>
    /// <response code="200">Lista de evaluaciones obtenida correctamente.</response>
    /// <response code="404">Materia no encontrada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EvaluacionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(Guid materiaId, CancellationToken ct)
    {
        try
        {
            var dtos = await mediator.Send(new GetEvaluacionesQuery(materiaId), ct);
            return Ok(dtos);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Agrega una evaluación a la materia indicada.
    /// </summary>
    /// <param name="materiaId">Id de la materia.</param>
    /// <param name="command">Datos de la evaluación.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>201 Created con el DTO de la evaluación creada.</returns>
    /// <response code="201">Evaluación creada correctamente.</response>
    /// <response code="400">Datos de validación inválidos o Id en URL no coincide.</response>
    /// <response code="404">Materia no encontrada.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluacionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Agregar(
        Guid materiaId,
        [FromBody] AgregarEvaluacionCommand command,
        CancellationToken ct)
    {
        if (materiaId != command.MateriaId)
            return BadRequest("El Id de la materia en la URL no coincide con el del cuerpo.");

        try
        {
            var dto = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(Listar), new { materiaId }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
