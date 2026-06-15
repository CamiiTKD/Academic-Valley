using FluentValidation;

namespace AcademicPlanner.Application.Features.Agenda.Commands.AgregarHorario;

public sealed class AgregarHorarioValidator : AbstractValidator<AgregarHorarioCommand>
{
    public AgregarHorarioValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty().WithMessage("El Id de la materia es requerido.");

        RuleFor(x => x)
            .Must(cmd => cmd.HoraFin > cmd.HoraInicio)
            .WithMessage("La hora de fin debe ser posterior a la hora de inicio.");

        RuleFor(x => x.Aula)
            .MaximumLength(100).WithMessage("El aula no puede superar los 100 caracteres.")
            .When(x => x.Aula is not null);
    }
}
