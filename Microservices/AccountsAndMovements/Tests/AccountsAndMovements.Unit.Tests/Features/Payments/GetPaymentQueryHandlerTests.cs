using AccountsAndMovements.Application.Features.Payments.Queries.GetPayment;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Payments;

public class GetPaymentQueryHandlerTests
{
    private const string TransactionCode = "d3f1b6f0-0000-4000-8000-000000000001";

    private readonly IMovementRepository _repository = Substitute.For<IMovementRepository>();
    private readonly GetPaymentQueryHandler _handler;

    public GetPaymentQueryHandlerTests()
        => _handler = new GetPaymentQueryHandler(_repository);

    [Fact]
    public async Task Handle_ReturnsTheOperationWithBothLegs()
    {
        GivenTransaction();

        var result = await _handler.Handle(new GetPaymentQuery(TransactionCode, string.Empty), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.ClientAccountNumber.Should().Be("QR0000000001");
        result.Data.CommerceAccountNumber.Should().Be("QR0000000002");
        result.Data.ConvertedAmount.Should().Be(139.20m);
        result.Data.ExchangeRate.Should().Be(6.96m);
    }

    [Fact]
    public async Task Handle_WhenOnlyTheIdempotencyKeyIsKnown_ResolvesTheOperation()
    {
        _repository.GetByIdempotencyKey("TX-2026-000001", Arg.Any<CancellationToken>())
            .Returns(Debit());
        GivenTransaction();

        var result = await _handler.Handle(
            new GetPaymentQuery(string.Empty, "TX-2026-000001"), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.TransactionCode.Should().Be(TransactionCode);
    }

    [Fact]
    public async Task Handle_WhenTheOperationDoesNotExist_ReturnsPaymentNotFound()
    {
        _repository.GetByTransactionCode(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await _handler.Handle(new GetPaymentQuery(TransactionCode, string.Empty), default);

        result.StatusCode.Should().Be(ErrorCode.PAYMENT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenTheKeyWasNeverUsed_ReturnsPaymentNotFound()
    {
        _repository.GetByIdempotencyKey(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((MovementEntity?)null);

        var result = await _handler.Handle(new GetPaymentQuery(string.Empty, "TX-NUNCA"), default);

        result.StatusCode.Should().Be(ErrorCode.PAYMENT_NOT_FOUND);
    }

    private void GivenTransaction()
        => _repository.GetByTransactionCode(TransactionCode, Arg.Any<CancellationToken>())
            .Returns([Debit(), Credit()]);

    private static MovementEntity Debit() => new()
    {
        MovementId       = 900,
        AccountNumber    = "QR0000000001",
        AccountType      = AccountType.CLIENT,
        HolderId         = 7,
        Type             = MovementType.DEBITO,
        Status           = MovementStatus.COMPLETED,
        Amount           = 20m,
        BalanceAfter     = 480m,
        OriginalAmount   = 20m,
        OriginalCurrency = "USD",
        ExchangeRate     = 6.96m,
        ConvertedAmount  = 139.20m,
        TargetCurrency   = "BOB",
        CommerceId       = 55,
        QrCode           = "QR-BO-0001",
        IdempotencyKey   = "TX-2026-000001",
        TransactionCode  = TransactionCode
    };

    private static MovementEntity Credit() => new()
    {
        MovementId      = 901,
        AccountNumber   = "QR0000000002",
        AccountType     = AccountType.COMMERCE,
        HolderId        = 55,
        Type            = MovementType.CREDITO,
        Status          = MovementStatus.COMPLETED,
        Amount          = 139.20m,
        ConvertedAmount = 139.20m,
        TargetCurrency  = "BOB",
        TransactionCode = TransactionCode
    };
}
