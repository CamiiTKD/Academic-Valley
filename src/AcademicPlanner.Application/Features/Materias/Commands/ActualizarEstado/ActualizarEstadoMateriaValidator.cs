using FluentValidation;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;

public sealed class ActualizarEstadoMateriaValidator : AbstractValidator<ActualizarEstadoMateriaCommand>
{
    public ActualizarEstadoMateriaValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty()
            .WithMessage("El Id de la materia es requerido.");
    }
}
