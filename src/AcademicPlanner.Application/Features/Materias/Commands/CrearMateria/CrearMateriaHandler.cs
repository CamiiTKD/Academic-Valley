using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.DTOs;
using AcademicPlanner.Domain.Entities;
using MediatR;

namespace AcademicPlanner.Application.Features.Materias.Commands.CrearMateria;

public sealed class CrearMateriaHandler(
    IMateriaRepository materiaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CrearMateriaCommand, MateriaDto>
{
    public async Task<MateriaDto> Handle(CrearMateriaCommand request, CancellationToken cancellationToken)
    {
        var materia = Materia.Crear(request.Nombre, request.Codigo, request.Cuatrimestre);

        if (request.EstadoInicial != Domain.Enums.EstadoMateria.Pendiente)
            materia.ActualizarEstado(request.EstadoInicial, request.NotaFinal);

        if (request.CorrelativasIds is { Count: > 0 })
        {
            var correlativas = await materiaRepository.GetByIdsAsync(request.CorrelativasIds, cancellationToken);

            var idsNoEncontrados = request.CorrelativasIds
                .Except(correlativas.Select(c => c.Id))
                .ToList();

            if (idsNoEncontrados.Count > 0)
                throw new KeyNotFoundException(
                    $"No se encontraron las siguientes correlativas: {string.Join(", ", idsNoEncontrados)}.");

            foreach (var correlativa in correlativas)
                materia.AgregarCorrelativa(correlativa);
        }

        await materiaRepository.AddAsync(materia, cancellationToken);
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
                .AsReadOnly()
        );
    }
}
