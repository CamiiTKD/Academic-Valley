using AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;
using AcademicPlanner.Application.Features.Materias.Commands.CrearMateria;
using AcademicPlanner.Application.Features.Materias.Commands.EliminarMateria;
using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Application.Features.Materias.Queries.GetMaterias;
using AcademicPlanner.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AcademicPlanner.API.Controllers;

/// <summary>
/// Gestión de materias del plan de estudio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MateriasController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Crea una nueva materia en el plan de estudios.
    /// </summary>
    /// <param name="command">Datos de la materia a crear.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>201 Created con el DTO de la materia creada.</returns>
    /// <response code="201">Materia creada correctamente.</response>
    /// <response code="400">Datos de validación inválidos.</response>
    /// <response code="404">Una o más correlativas indicadas no existen.</response>
    [HttpPost]
    [ProducesResponseType(typeof(MateriaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Crear([FromBody] CrearMateriaCommand command, CancellationToken ct)
    {
        try
        {
            var dto = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(Listar), null, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Lista todas las materias del plan de estudios, incluyendo sus correlativas.
    /// </summary>
    /// <param name="estado">Filtro opcional por estado (Pendiente, Cursando, Regular, Aprobada).</param>
    /// <param name="cuatrimestre">Filtro opcional por número de cuatrimestre.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>200 OK con la lista de materias.</returns>
    /// <response code="200">Lista de materias obtenida correctamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MateriaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] EstadoMateria? estado,
        [FromQuery] int? cuatrimestre,
        CancellationToken ct)
    {
        var dtos = await mediator.Send(new GetMateriasQuery(estado, cuatrimestre), ct);
        return Ok(dtos);
    }

    /// <summary>
    /// Elimina una materia del plan de estudios.
    /// </summary>
    /// <param name="id">Id único de la materia a eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>204 NoContent si fue exitoso.</returns>
    /// <response code="204">Materia eliminada correctamente.</response>
    /// <response code="404">Materia no encontrada.</response>
    /// <response code="409">La materia es correlativa requerida de otras materias y no puede eliminarse.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new EliminarMateriaCommand(id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza el estado de una materia (ej: marcarla como Aprobada con su nota final).
    /// </summary>
    /// <param name="id">Id único de la materia.</param>
    /// <param name="command">Datos para la actualización.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>204 NoContent si fue exitoso.</returns>
    /// <response code="204">Estado actualizado correctamente.</response>
    /// <response code="400">Datos de validación inválidos (ej: nota fuera de rango).</response>
    /// <response code="404">Materia no encontrada.</response>
    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActualizarEstado(
        Guid id,
        [FromBody] ActualizarEstadoMateriaCommand command,
        CancellationToken ct)
    {
        if (id != command.MateriaId)
            return BadRequest("El Id en la URL no coincide con el Id en el cuerpo.");

        await mediator.Send(command, ct);
        return NoContent();
    }
}
