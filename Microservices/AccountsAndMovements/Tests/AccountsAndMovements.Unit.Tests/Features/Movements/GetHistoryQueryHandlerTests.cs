using AccountsAndMovements.Application.Features.Movements.Queries.GetHistory;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AccountsAndMovements.Unit.Tests.Features.Movements;

public class GetHistoryQueryHandlerTests
{
    private readonly IMovementRepository _repository = Substitute.For<IMovementRepository>();
    private readonly GetHistoryQueryHandler _handler;

    public GetHistoryQueryHandlerTests()
        => _handler = new GetHistoryQueryHandler(_repository);

    [Fact]
    public async Task Handle_ReturnsTheMovementsWithTheConversionOfEachOperation()
    {
        _repository.GetHistory(
            Arg.Any<string>(), Arg.Any<long>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([
                new MovementEntity
                {
                    MovementId = 900, AccountNumber = "QR0000000001", Type = MovementType.DEBITO,
                    Status = MovementStatus.COMPLETED, Amount = 20m,
                    OriginalAmount = 20m, OriginalCurrency = "USD", ExchangeRate = 6.96m,
                    ConvertedAmount = 139.20m, TargetCurrency = "BOB"
                }
            ]);

        var result = await _handler.Handle(
            new GetHistoryQuery(AccountType.CLIENT, 7, null, null, null, 1, 20), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().HaveCount(1);
        result.Data![0].ExchangeRate.Should().Be(6.96m);
        result.Data[0].ConvertedAmount.Should().Be(139.20m);
    }

    [Fact]
    public async Task Handle_WhenTheEndDateHasNoTime_IncludesTheWholeLastDay()
    {
        DateTime? capturedTo = null;
        _repository.GetHistory(
            Arg.Any<string>(), Arg.Any<long>(), Arg.Any<DateTime?>(),
            Arg.Do<DateTime?>(x => capturedTo = x),
            Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(
            new GetHistoryQuery(AccountType.CLIENT, 7,
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 30), null, 1, 20), default);

        // La consulta filtra con "menor que": sin correr el limite un día, todo
        // lo del 30 de septiembre quedaría fuera del resultado.
        capturedTo.Should().Be(new DateTime(2026, 10, 1));
    }

    [Fact]
    public async Task Handle_WhenTheEndDateHasTime_UsesItAsIs()
    {
        DateTime? capturedTo = null;
        _repository.GetHistory(
            Arg.Any<string>(), Arg.Any<long>(), Arg.Any<DateTime?>(),
            Arg.Do<DateTime?>(x => capturedTo = x),
            Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(
            new GetHistoryQuery(AccountType.CLIENT, 7, null,
                new DateTime(2026, 9, 30, 18, 0, 0), null, 1, 20), default);

        capturedTo.Should().Be(new DateTime(2026, 9, 30, 18, 0, 0));
    }

    [Fact]
    public async Task Handle_NormalizesTheStatusFilterAndTreatsBlankAsNoFilter()
    {
        string? capturedStatus = "sin asignar";
        _repository.GetHistory(
            Arg.Any<string>(), Arg.Any<long>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
            Arg.Do<string?>(x => capturedStatus = x),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _handler.Handle(
            new GetHistoryQuery(AccountType.CLIENT, 7, null, null, "   ", 1, 20), default);

        capturedStatus.Should().BeNull();

        await _handler.Handle(
            new GetHistoryQuery(AccountType.CLIENT, 7, null, null, "completed", 1, 20), default);

        capturedStatus.Should().Be(MovementStatus.COMPLETED);
    }
}
