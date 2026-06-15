using FluentValidation;

namespace AcademicPlanner.Application.Features.Materias.Commands.EliminarMateria;

public sealed class EliminarMateriaValidator : AbstractValidator<EliminarMateriaCommand>
{
    public EliminarMateriaValidator()
    {
        RuleFor(x => x.MateriaId)
            .NotEmpty()
            .WithMessage("El Id de la materia es requerido.");
    }
}
