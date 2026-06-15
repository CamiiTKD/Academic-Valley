using AcademicPlanner.Application.Features.Materias.Commands.CrearMateria;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class CrearMateriaValidatorTests
{
    private readonly CrearMateriaValidator _sut = new();

    [Fact]
    public void Validar_NombreVacio_RetornaError()
    {
        var command = new CrearMateriaCommand("", "MAT1", 1, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Nombre));
    }

    [Fact]
    public void Validar_NombreNulo_RetornaError()
    {
        var command = new CrearMateriaCommand(null!, "MAT1", 1, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Nombre));
    }

    [Fact]
    public void Validar_CodigoVacio_RetornaError()
    {
        var command = new CrearMateriaCommand("Matemática I", "", 1, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Codigo));
    }

    [Fact]
    public void Validar_CodigoExcedeVeinteCaracteres_RetornaError()
    {
        var command = new CrearMateriaCommand("Matemática I", new string('X', 21), 1, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Codigo));
    }

    [Fact]
    public void Validar_CuatrimestreCero_RetornaError()
    {
        var command = new CrearMateriaCommand("Matemática I", "MAT1", 0, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Cuatrimestre));
    }

    [Fact]
    public void Validar_CorrelativaConGuidVacio_RetornaError()
    {
        var command = new CrearMateriaCommand("Análisis II", "ANA2", 2, [Guid.Empty]);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains(nameof(command.CorrelativasIds)));
    }

    [Fact]
    public void Validar_DatosValidosSinCorrelativas_SinErrores()
    {
        var command = new CrearMateriaCommand("Álgebra Lineal", "ALG1", 1, null);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DatosValidosConCorrelativasValidas_SinErrores()
    {
        var command = new CrearMateriaCommand("Análisis II", "ANA2", 2, [Guid.NewGuid(), Guid.NewGuid()]);
        var result = _sut.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    // Regla de dominio: una materia no puede ser correlativa de sí misma
    [Fact]
    public void AgregarCorrelativa_MateriaMismoId_LanzaInvalidOperationException()
    {
        var materia = Materia.Crear("Análisis I", "ANA1", 1);

        var act = () => materia.AgregarCorrelativa(materia);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no puede ser correlativa de sí misma*");
    }
}
