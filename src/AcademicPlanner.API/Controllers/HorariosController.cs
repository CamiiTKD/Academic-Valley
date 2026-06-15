using AcademicPlanner.Application.Features.Agenda.Commands.ActualizarHorario;
using AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;
using AcademicPlanner.Application.Features.Agenda.Commands.EliminarHorario;
using AcademicPlanner.Application.Features.Agenda.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.API.Controllers;

/// <summary>
/// Gestión de horarios de cursada de una materia.
/// </summary>
[ApiController]
[Route("api/Materias/{materiaId:guid}/horarios")]
[Produces("application/json")]
public class HorariosController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Agrega un horario de cursada a la materia indicada.
    /// </summary>
    /// <param name="materiaId">Id de la materia.</param>
    /// <param name="command">Datos del horario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>201 Created con el DTO del horario creado.</returns>
    /// <response code="201">Horario creado correctamente.</response>
    /// <response code="400">Datos de validación inválidos o Id en URL no coincide.</response>
    /// <response code="404">Materia no encontrada.</response>
    [HttpPost]
    [ProducesResponseType(typeof(HorarioCursadaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Agregar(
        Guid materiaId,
        [FromBody] AgregarHorarioCommand command,
        CancellationToken ct)
    {
        if (materiaId != command.MateriaId)
            return BadRequest("El Id de la materia en la URL no coincide con el del cuerpo.");

        try
        {
            var dto = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(Agregar), new { materiaId, horarioId = dto.Id }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza un horario de cursada existente.
    /// </summary>
    /// <param name="materiaId">Id de la materia.</param>
    /// <param name="horarioId">Id del horario a actualizar.</param>
    /// <param name="command">Nuevos datos del horario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>204 NoContent si fue exitoso.</returns>
    /// <response code="204">Horario actualizado correctamente.</response>
    /// <response code="400">Datos de validación inválidos o Ids en URL no coinciden.</response>
    /// <response code="404">Materia o horario no encontrado.</response>
    [HttpPut("{horarioId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(
        Guid materiaId,
        Guid horarioId,
        [FromBody] ActualizarHorarioCommand command,
        CancellationToken ct)
    {
        if (materiaId != command.MateriaId || horarioId != command.HorarioId)
            return BadRequest("Los Ids en la URL no coinciden con los del cuerpo.");

        try
        {
            await mediator.Send(command, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Elimina un horario de cursada de la materia indicada.
    /// </summary>
    /// <param name="materiaId">Id de la materia.</param>
    /// <param name="horarioId">Id del horario a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>204 NoContent si fue exitoso.</returns>
    /// <response code="204">Horario eliminado correctamente.</response>
    /// <response code="404">Materia o horario no encontrado.</response>
    [HttpDelete("{horarioId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid materiaId, Guid horarioId, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new EliminarHorarioCommand(materiaId, horarioId), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
