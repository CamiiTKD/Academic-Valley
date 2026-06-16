using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.Commands.ActualizarMateria;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class ActualizarMateriaHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ActualizarMateriaHandler _sut;

    public ActualizarMateriaHandlerTests()
    {
        _sut = new ActualizarMateriaHandler(_repository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdWithCorrelativasAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaExiste_RetornaDtoConNuevosDatos()
    {
        var materia = Materia.Crear("Nombre Viejo", "OLD1", 1);
        _repository.GetByIdWithCorrelativasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new ActualizarMateriaCommand(materia.Id, "Nombre Nuevo", "NEW1", 2, EstadoMateria.Cursando);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Nombre.Should().Be("Nombre Nuevo");
        result.Codigo.Should().Be("NEW1");
        result.Cuatrimestre.Should().Be(2);
        result.Estado.Should().Be(EstadoMateria.Cursando);
    }

    [Fact]
    public async Task Handle_MateriaExiste_InvocaUpdateYSaveChanges()
    {
        var materia = Materia.Crear("Análisis I", "ANA1", 1);
        _repository.GetByIdWithCorrelativasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        _repository.Received(1).Update(materia);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CodigoEnMinusculas_RetornaCodigoEnMayusculas()
    {
        var materia = Materia.Crear("Física", "FIS1", 1);
        _repository.GetByIdWithCorrelativasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new ActualizarMateriaCommand(materia.Id, "Física General", "fis2", 2, EstadoMateria.Pendiente);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Codigo.Should().Be("FIS2");
    }

    [Fact]
    public async Task Handle_MateriaExiste_RetornaDtoConCorrelativasYNotasVacias()
    {
        var materia = Materia.Crear("Sistemas", "SIS1", 3);
        _repository.GetByIdWithCorrelativasAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var result = await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        result.Correlativas.Should().BeEmpty();
        result.RegistroNotas.Should().BeEmpty();
    }

    private static ActualizarMateriaCommand ComandoValido(Guid id) =>
        new(id, "Sistemas Operativos", "SO1", 3, EstadoMateria.Cursando);
}
