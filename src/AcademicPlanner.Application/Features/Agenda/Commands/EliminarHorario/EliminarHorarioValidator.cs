using FluentValidation;

namespace AcademicPlanner.Application.Features.Agenda.Commands.EliminarHorario;

public sealed class EliminarHorarioValidator : AbstractValidator<EliminarHorarioCommand>
{
    public EliminarHorarioValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty().WithMessage("El Id de la materia es requerido.");

        RuleFor(x => x.HorarioId)
            .NotEmpty().WithMessage("El Id del horario es requerido.");
    }
}
