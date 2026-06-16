using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Notas.DTOs;
using AcademicPlanner.Domain.Entities;
using MediatR;

namespace AcademicPlanner.Application.Features.Notas.Commands.AgregarNota;

public sealed class AgregarNotaHandler(
    IMateriaRepository materiaRepository,
    IRegistroNotaRepository registroNotaRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AgregarNotaCommand, RegistroNotaDto>
{
    public async Task<RegistroNotaDto> Handle(AgregarNotaCommand request, CancellationToken cancellationToken)
    {
        var existe = await materiaRepository.GetByIdAsync(request.MateriaId, cancellationToken)
            ?? throw new KeyNotFoundException($"No se encontró la materia con Id {request.MateriaId}.");

        var nota = RegistroNota.Crear(existe.Id, request.ValorNota, request.Fecha, request.Tipo);

        await registroNotaRepository.AddAsync(nota, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegistroNotaDto(nota.Id, nota.ValorNota, nota.Fecha, nota.Tipo, nota.ValorNota < 4);
    }
}
