using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.Commands.ActualizarEstado;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class ActualizarEstadoMateriaHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ActualizarEstadoMateriaHandler _sut;

    public ActualizarEstadoMateriaHandlerTests()
    {
        _sut = new ActualizarEstadoMateriaHandler(_repository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(
            new ActualizarEstadoMateriaCommand(id, EstadoMateria.Cursando), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoLlamaUpdateNiSaveChanges()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(
            new ActualizarEstadoMateriaCommand(Guid.NewGuid(), EstadoMateria.Cursando), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();

        _repository.DidNotReceive().Update(Arg.Any<Materia>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaExiste_ActualizaEstadoYGuarda()
    {
        var materia = Materia.Crear("Bases de Datos", "BD1", 3);
        _repository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(
            new ActualizarEstadoMateriaCommand(materia.Id, EstadoMateria.Cursando), CancellationToken.None);

        materia.Estado.Should().Be(EstadoMateria.Cursando);
        _repository.Received(1).Update(materia);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CambioAAprobada_EstadoCorrecto()
    {
        var materia = Materia.Crear("Redes", "RED1", 4);
        materia.ActualizarEstado(EstadoMateria.Regular);
        _repository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(
            new ActualizarEstadoMateriaCommand(materia.Id, EstadoMateria.Aprobada), CancellationToken.None);

        materia.Estado.Should().Be(EstadoMateria.Aprobada);
    }
}
