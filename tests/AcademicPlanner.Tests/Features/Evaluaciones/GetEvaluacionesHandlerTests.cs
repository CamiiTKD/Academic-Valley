using AcademicPlanner.Application.Common.Interfaces;
using AcademicPlanner.Application.Features.Evaluaciones.Queries.GetEvaluaciones;
using AcademicPlanner.Domain.Entities;
using AcademicPlanner.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AcademicPlanner.Tests.Features.Evaluaciones;

public sealed class GetEvaluacionesHandlerTests
{
    private readonly IMateriaRepository _materiaRepository = Substitute.For<IMateriaRepository>();
    private readonly IEvaluacionRepository _evaluacionRepository = Substitute.For<IEvaluacionRepository>();
    private readonly GetEvaluacionesHandler _sut;

    public GetEvaluacionesHandlerTests()
    {
        _sut = new GetEvaluacionesHandler(_materiaRepository, _evaluacionRepository);
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_LanzaKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _materiaRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(new GetEvaluacionesQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage($"*{id}*");
    }

    [Fact]
    public async Task Handle_MateriaNoExiste_NoConsultaRepositorioDeEvaluaciones()
    {
        _materiaRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Materia?)null);

        var act = async () => await _sut.Handle(new GetEvaluacionesQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();

        await _evaluacionRepository.DidNotReceive()
            .GetByMateriaIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MateriaExisteSinEvaluaciones_RetornaListaVacia()
    {
        var materia = Materia.Crear("Redes", "RED1", 4);
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);
        _evaluacionRepository.GetByMateriaIdAsync(materia.Id, Arg.Any<CancellationToken>())
            .Returns(new List<Evaluacion>().AsReadOnly() as IReadOnlyList<Evaluacion>);

        var result = await _sut.Handle(new GetEvaluacionesQuery(materia.Id), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MateriaConEvaluaciones_RetornaDtosConDatosCorrectos()
    {
        var materia = Materia.Crear("Seguridad", "SEG1", 5);
        var ev1 = Evaluacion.Crear(materia.Id, TipoEvaluacion.Parcial, new DateOnly(2026, 8, 5), "Primer parcial");
        var ev2 = Evaluacion.Crear(materia.Id, TipoEvaluacion.Final, new DateOnly(2026, 12, 1), null);
        IReadOnlyList<Evaluacion> lista = [ev1, ev2];
        _materiaRepository.GetByIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(materia);
        _evaluacionRepository.GetByMateriaIdAsync(materia.Id, Arg.Any<CancellationToken>()).Returns(lista);

        var result = await _sut.Handle(new GetEvaluacionesQuery(materia.Id), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].MateriaId.Should().Be(materia.Id);
        result[0].Tipo.Should().Be(TipoEvaluacion.Parcial);
        result[0].Estado.Should().Be(EstadoEvaluacion.Pendiente);
        result[0].Nota.Should().BeNull();
        result[1].Tipo.Should().Be(TipoEvaluacion.Final);
        result[1].Descripcion.Should().BeNull();
    }
}
