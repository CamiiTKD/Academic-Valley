using FluentValidation;

namespace AcademicPlanner.Application.Features.Materias.Commands.ActualizarMateria;

public sealed class ActualizarMateriaValidator : AbstractValidator<ActualizarMateriaCommand>
{
    public ActualizarMateriaValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("El Id de la materia es requerido.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es requerido.")
            .MaximumLength(200).WithMessage("El nombre no puede superar los 200 caracteres.");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es requerido.")
            .MaximumLength(20).WithMessage("El código no puede superar los 20 caracteres.");

        RuleFor(x => x.Cuatrimestre)
            .GreaterThan(0).WithMessage("El cuatrimestre debe ser mayor a 0.");
    }
}
