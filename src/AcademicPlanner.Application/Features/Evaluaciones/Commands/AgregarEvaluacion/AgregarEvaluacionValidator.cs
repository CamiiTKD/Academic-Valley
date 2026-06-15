using FluentValidation;

namespace AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;

public sealed class AgregarEvaluacionValidator : AbstractValidator<AgregarEvaluacionCommand>
{
    public AgregarEvaluacionValidator()
    {
        RuleFor(x => x.MateriaId).NotEmpty().WithMessage("El Id de la materia es requerido.");

        RuleFor(x => x.Fecha).NotEmpty().WithMessage("La fecha de la evaluación es requerida.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("La descripción no puede superar los 500 caracteres.")
            .When(x => x.Descripcion is not null);
    }
}
