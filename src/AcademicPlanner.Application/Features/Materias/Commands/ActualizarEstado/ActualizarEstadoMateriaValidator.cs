using AcademicPlanner.Domain.Enums;
using FluentValidation;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;

public sealed class ActualizarEstadoMateriaValidator : AbstractValidator<ActualizarEstadoMateriaCommand>
{
    public ActualizarEstadoMateriaValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty()
            .WithMessage("El Id de la materia es requerido.");

        // Las reglas sobre NotaFinal solo aplican cuando se quiere marcar como Aprobada
        When(x => x.NuevoEstado == EstadoMateria.Aprobada, () =>
        {
            RuleFor(x => x.NotaFinal)
                .NotNull()
                .WithMessage("La nota final es obligatoria para aprobar una materia.")
                .InclusiveBetween(1m, 10m)
                .WithMessage("La nota final debe estar entre 1 y 10.");
        });

        // No tiene sentido pasar una nota cuando el estado es Pendiente
        When(x => x.NuevoEstado == EstadoMateria.Pendiente, () =>
        {
            RuleFor(x => x.NotaFinal)
                .Null()
                .WithMessage("No se puede registrar una nota para una materia en estado Pendiente.");
        });
    }
}
