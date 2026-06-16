using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Evaluaciones.Commands.AgregarEvaluacion;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Evaluaciones;

public sealed class AgregarEvaluacionHandlerTests
{
    private readonly IMateriaRepository _materiaRepository = Substitute.For<IMateriaRepository>();
    private readonly IEvaluacionRepository _evaluacionRepository = Substitute.For<IEvaluacionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AgregarEvaluacionHandler _sut;

    public AgregarEvaluacionHandlerTests()
    {
        _sut = new AgregarEvaluacionHandler(_materiaRepository, _evaluacionRepository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _materiaRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoInvocaAddNiSaveChanges()
    {
        _materiaRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(ComandoValido(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        await _evaluacionRepository.DidNotReceive().AddAsync(Arg.Any<Evaluacion>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaExiste_RetornaDtoConDatosCorrectos()
    {
        var materia = Materia.Crear("Algoritmos", "ALG1", 1);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = ComandoValido(materia.Id);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Id.Should().NotBe(Guid.Empty);
        result.MateriaId.Should().Be(materia.Id);
        result.Tipo.Should().Be(command.Tipo);
        result.Fecha.Should().Be(command.Fecha);
        result.Descripcion.Should().Be(command.Descripcion);
        result.Estado.Should().Be(EstadoEvaluacion.Pendiente);
        result.Nota.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MateriaExiste_InvocaAddYSaveChanges()
    {
        var materia = Materia.Crear("Algoritmos", "ALG1", 1);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        await _sut.Handle(ComandoValido(materia.Id), CancellationToken.None);

        await _evaluacionRepository.Received(1).AddAsync(Arg.Any<Evaluacion>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DescripcionNula_RetornaDtoConDescripcionNula()
    {
        var materia = Materia.Crear("Algoritmos", "ALG1", 1);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);

        var command = new AgregarEvaluacionCommand(materia.Id, TipoEvaluacion.Parcial, new DateOnly(2026, 7, 10), null);
        var result = await _sut.Handle(command, CancellationToken.None);

        result.Descripcion.Should().BeNull();
    }

    private static AgregarEvaluacionCommand ComandoValido(Guid materiaId) =>
        new(materiaId, TipoEvaluacion.Parcial, new DateOnly(2026, 7, 10), "Primer parcial");
}
