using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Materias.Queries.GetMaterias;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Materias;

public sealed class GetMateriasHandlerTests
{
    private readonly IMateriaRepository _repository = Substitute.For<IMateriaRepository>();
    private readonly GetMateriasHandler _sut;

    public GetMateriasHandlerTests()
    {
        _sut = new GetMateriasHandler(_repository);
    }

    [Fact]
    public async Task Handle_SinFiltros_RetornaTodas()
    {
        var m1 = Materia.Crear("Álgebra", "ALG1", 1);
        var m2 = Materia.Crear("Análisis", "ANA1", 2);
        IReadOnlyList<Materia> lista = [m1, m2];
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetMateriasQuery(null, null), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_FiltroEstado_RetornaSoloMatching()
    {
        var cursando = Materia.Crear("Física", "FIS1", 1);
        cursando.ActualizarEstado(EstadoMateria.Cursando);
        var pendiente = Materia.Crear("Química", "QUI1", 2);
        IReadOnlyList<Materia> lista = [cursando, pendiente];
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetMateriasQuery(EstadoMateria.Cursando, null), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Estado.Should().Be(EstadoMateria.Cursando);
    }

    [Fact]
    public async Task Handle_FiltroCuatrimestre_RetornaSoloMatching()
    {
        var m1 = Materia.Crear("Álgebra", "ALG1", 1);
        var m2 = Materia.Crear("Análisis", "ANA1", 2);
        var m3 = Materia.Crear("Física", "FIS1", 1);
        IReadOnlyList<Materia> lista = [m1, m2, m3];
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetMateriasQuery(null, 1), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(m => m.Cuatrimestre.Should().Be(1));
    }

    [Fact]
    public async Task Handle_FiltrosAmbos_AplicaAmbos()
    {
        var m1 = Materia.Crear("Álgebra", "ALG1", 1);
        m1.ActualizarEstado(EstadoMateria.Aprobada);
        var m2 = Materia.Crear("Física", "FIS1", 1);
        var m3 = Materia.Crear("Análisis", "ANA1", 2);
        m3.ActualizarEstado(EstadoMateria.Aprobada);
        IReadOnlyList<Materia> lista = [m1, m2, m3];
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetMateriasQuery(EstadoMateria.Aprobada, 1), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Nombre.Should().Be("Álgebra");
    }

    [Fact]
    public async Task Handle_RepositorioVacio_RetornaListaVacia()
    {
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Materia>().AsReadOnly() as IReadOnlyList<Materia>);

        var result = await _sut.Handle(new GetMateriasQuery(null, null), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RetornaDto_ConCamposCorrectos()
    {
        var materia = Materia.Crear("Sistemas Operativos", "SO1", 3);
        IReadOnlyList<Materia> lista = [materia];
        _repository.GetAllWithCorrelativasAndNotasAsync(Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetMateriasQuery(null, null), CancellationToken.None);

        var dto = result[0];
        dto.Id.Should().Be(materia.Id);
        dto.Nombre.Should().Be("Sistemas Operativos");
        dto.Codigo.Should().Be("SO1");
        dto.Cuatrimestre.Should().Be(3);
        dto.Estado.Should().Be(EstadoMateria.Pendiente);
        dto.Correlativas.Should().BeEmpty();
        dto.RegistroNotas.Should().BeEmpty();
    }
}
