using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Domain.Enums;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarMateria;

public sealed class ActualizarMateriaHandler(
    IMateriaRepository materiaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActualizarMateriaCommand, MateriaDto>
{
    public async Task<MateriaDto> Handle(ActualizarMateriaCommand request, CancellationToken cancellationToken)
    {
        var materia = await materiaRepository.GetByIdWithCorrelativasAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {request.Id}.");

        materia.Actualizar(request.Nombre, request.Codigo, request.Cuatrimestre);
        materia.ActualizarEstado(
            request.Estado,
            request.Estado == EstadoMateria.Aprobada ? request.NotaFinal : null);

        materiaRepository.Update(materia);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new MateriaDto(
            materia.Id,
            materia.Nombre,
            materia.Codigo,
            materia.Cuatrimestre,
            materia.NotaFinal,
            materia.Estado,
            materia.Correlativas
                .Select(c => new CorrelativaResumenDto(c.Id, c.Nombre, c.Codigo, c.Estado))
                .ToList()
                .AsReadOnly());
    }
}
