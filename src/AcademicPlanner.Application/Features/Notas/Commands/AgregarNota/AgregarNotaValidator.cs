using FluentValidation;

namespace AcademicPlanner.Application.Features.Notas.Commands.AgregarNota;

public sealed class AgregarNotaValidator : AbstractValidator<AgregarNotaCommand>
{
    public AgregarNotaValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty().WithMessage("El Id de la materia es requerido.");

        RuleFor(x => x.ValorNota)
            .InclusiveBetween(1, 10).WithMessage("La nota debe estar entre 1 y 10.");

        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es requerida.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("La fecha no puede ser futura.");
    }
}
