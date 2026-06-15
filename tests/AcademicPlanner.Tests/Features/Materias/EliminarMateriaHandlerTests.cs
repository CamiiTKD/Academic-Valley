using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.Commands.EliminarMateria;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class EliminarMateriaHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly EliminarMateriaHandler _sut;

    public EliminarMateriaHandlerTests()
    {
        _sut = new EliminarMateriaHandler(_repository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Materia?)null);

        var act = async () => await _sut.Handle(new EliminarMateriaCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaEsCorrelativaDeOtra_LanzaInvalidOperationException()
    {
        var materia = Materia.Crear("Análisis Matemático I", "ANA1", 1);
        _repository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(materia);
        _repository.ExistsAsCorrelativaAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(new EliminarMateriaCommand(materia.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*correlativa requerida*");
    }

    [Fact]
    public async Task Handle_MateriaEsCorrelativaDeOtra_NoLlamaDeleteNiSaveChanges()
    {
        var materia = Materia.Crear("Análisis Matemático I", "ANA1", 1);
        _repository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(materia);
        _repository.ExistsAsCorrelativaAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var act = async () => await _sut.Handle(new EliminarMateriaCommand(materia.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        _repository.DidNotReceive().Delete(Arg.Any<Materia>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaEliminable_InvocaDeleteYSaveChanges()
    {
        var materia = Materia.Crear("Introducción a la Programación", "PRG0", 1);
        _repository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(materia);
        _repository.ExistsAsCorrelativaAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _sut.Handle(new EliminarMateriaCommand(materia.Id), CancellationToken.None);

        _repository.Received(1).Delete(materia);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
