using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.Commands.CrearMateria;
using AcademicPlanner.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class CrearMateriaHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CrearMateriaHandler _sut;

    public CrearMateriaHandlerTests()
    {
        _sut = new CrearMateriaHandler(_repository, _unitOfWork);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    [Fact]
    public async Task Handle_CorrelativaNoEncontrada_LanzaKeyNotFoundException()
    {
        var idInexistente = Guid.NewGuid();
        _repository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Materia>().AsReadOnly() as IReadOnlyList<Materia>);

        var command = new CrearMateriaCommand("Análisis II", "ANA2", 2, [idInexistente]);
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{idInexistente}*");
    }

    [Fact]
    public async Task Handle_SinCorrelativas_RetornaDtoConDatosCorrectos()
    {
        var command = new CrearMateriaCommand("Álgebra Lineal", "ALG1", 1, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Nombre.Should().Be("Álgebra Lineal");
        result.Codigo.Should().Be("ALG1");
        result.Cuatrimestre.Should().Be(1);
        result.Correlativas.Should().BeEmpty();
        result.Id.Should().NotBe(Guid.Empty);

        await _repository.Received(1).AddAsync(Arg.Any<Materia>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConCorrelativas_RetornaDtoConCorrelativasMapeadas()
    {
        var correlativa = Materia.Crear("Matemática I", "MAT1", 1);
        IReadOnlyList<Materia> correlativas = [correlativa];
        _repository.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(correlativas);

        var command = new CrearMateriaCommand("Matemática II", "MAT2", 2, [correlativa.Id]);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Nombre.Should().Be("Matemática II");
        result.Correlativas.Should().HaveCount(1);
        result.Correlativas[0].Id.Should().Be(correlativa.Id);
        result.Correlativas[0].Nombre.Should().Be("Matemática I");
        result.Correlativas[0].Codigo.Should().Be("MAT1");
    }

    [Fact]
    public async Task Handle_SinCorrelativas_NoLlamaGetByIds()
    {
        var command = new CrearMateriaCommand("Física I", "FIS1", 1, null);

        await _sut.Handle(command, CancellationToken.None);

        await _repository.DidNotReceive()
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CodigoNormalizado_RetornaCodigoEnMayusculas()
    {
        var command = new CrearMateriaCommand("Química General", "qui1", 1, null);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Codigo.Should().Be("QUI1");
    }
}
