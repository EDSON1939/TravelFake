using AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Accounts;

public class ApplyMovementCommandHandlerTests
{
    private readonly IAccountRepository  _accountRepository  = Substitute.For<IAccountRepository>();
    private readonly IMovementRepository _movementRepository = Substitute.For<IMovementRepository>();
    private readonly ApplyMovementCommandHandler _handler;

    public ApplyMovementCommandHandlerTests()
        => _handler = new ApplyMovementCommandHandler(_accountRepository, _movementRepository);

    private static ApplyMovementCommand Command(
        string type = MovementType.CREDITO, decimal amount = 500m)
        => new("QR0000000001", type, amount, "RECARGA", "TOPUP-0001", "Recarga de saldo");

    [Fact]
    public async Task Handle_WhenMovementIsApplied_ReturnsTheMovementAndTheNewBalance()
    {
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(77L);
        _accountRepository.GetByNumber("QR0000000001", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity { Number = "QR0000000001", Balance = 2500m });

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.MovementId.Should().Be(77L);
        result.Data.Balance.Should().Be(2500m);
    }

    [Fact]
    public async Task Handle_NormalizesTheAccountNumberAndTheIdempotencyKey()
    {
        MovementEntity? captured = null;
        _movementRepository.ApplyMovement(
            Arg.Do<MovementEntity>(x => captured = x), Arg.Any<CancellationToken>()).Returns(1L);

        await _handler.Handle(
            new ApplyMovementCommand(" qr0000000001 ", MovementType.DEBITO, 10m,
                " REF ", "  TOPUP-1  ", " glosa "), default);

        captured!.AccountNumber.Should().Be("QR0000000001");
        captured.IdempotencyKey.Should().Be("TOPUP-1");
        captured.Reference.Should().Be("REF");
        captured.Description.Should().Be("glosa");
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ReturnsAccountNotFound()
    {
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.ACCOUNT_NOT_FOUND);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_NOT_FOUND);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenAccountIsInactive_ReturnsAccountInactive()
    {
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.ACCOUNT_INACTIVE);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenBalanceIsInsufficient_ReturnsInsufficientFunds()
    {
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.INSUFFICIENT_FUNDS);

        var result = await _handler.Handle(Command(MovementType.DEBITO, 9_000m), default);

        result.StatusCode.Should().Be(ErrorCode.INSUFFICIENT_FUNDS);
    }

    [Fact]
    public async Task Handle_WhenMovementFails_DoesNotReadTheAccount()
    {
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.INSUFFICIENT_FUNDS);

        await _handler.Handle(Command(MovementType.DEBITO), default);

        await _accountRepository.DidNotReceive().GetByNumber(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
