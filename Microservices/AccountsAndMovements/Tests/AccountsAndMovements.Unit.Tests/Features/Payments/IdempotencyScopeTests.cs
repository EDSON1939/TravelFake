using AccountsAndMovements.Application.Features.Accounts.Commands.ApplyMovement;
using AccountsAndMovements.Application.Features.Payments.Commands.ExecuteQrPayment;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Payments;

/// <summary>
/// La clave de idempotencia identifica una operacion del sistema entero, no una
/// operacion dentro de una cuenta. Estas pruebas fijan ese alcance: reusarla
/// para otra cosa se rechaza, y nunca devuelve el movimiento de un tercero.
/// </summary>
public class IdempotencyScopeTests
{
    private readonly IAccountRepository  _accountRepository  = Substitute.For<IAccountRepository>();
    private readonly IMovementRepository _movementRepository = Substitute.For<IMovementRepository>();
    private readonly IClientService      _clientService      = Substitute.For<IClientService>();
    private readonly IMerchantService    _merchantService    = Substitute.For<IMerchantService>();
    private readonly IQrService          _qrService          = Substitute.For<IQrService>();
    private readonly ICurrencyService    _currencyService    = Substitute.For<ICurrencyService>();

    private readonly ExecuteQrPaymentCommandHandler _paymentHandler;

    public IdempotencyScopeTests()
        => _paymentHandler = new ExecuteQrPaymentCommandHandler(
            _accountRepository, _movementRepository, _clientService, _merchantService,
            _qrService, _currencyService,
            NullLogger<ExecuteQrPaymentCommandHandler>.Instance);

    [Fact]
    public async Task Payment_WhenTheKeyBelongsToAnotherClient_IsRejectedInsteadOfReplayed()
    {
        // El movimiento guardado con esa clave es de OTRO cliente. Devolverlo
        // seria mostrarle a este cliente el pago de un tercero.
        _movementRepository.GetByIdempotencyKey("TX-2026-000001", Arg.Any<CancellationToken>())
            .Returns(new MovementEntity
            {
                MovementId     = 500,
                OwnerId        = 99,
                OwnerType      = AccountOwnerType.CLIENTE,
                Type           = MovementType.DEBITO,
                QrCode         = "QR-OTRO",
                OriginalAmount = 20m,
                OriginalCurrency = "USD",
                IdempotencyKey = "TX-2026-000001"
            });

        var result = await _paymentHandler.Handle(
            new ExecuteQrPaymentCommand(7, "QR-BO-0001", 20m, "USD", "TX-2026-000001", "Pago"),
            default);

        result.StatusCode.Should().Be(ErrorCode.DUPLICATE_TRANSACTION);
        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Payment_WhenTheKeyWasUsedForATopUp_IsRejectedInsteadOfReplayed()
    {
        // Misma cuenta, pero la clave se gasto en una recarga: dar por pagado un
        // QR que nunca se cobro seria regalarle la compra al cliente.
        _movementRepository.GetByIdempotencyKey("TX-2026-000001", Arg.Any<CancellationToken>())
            .Returns(new MovementEntity
            {
                MovementId     = 501,
                OwnerId        = 7,
                OwnerType      = AccountOwnerType.CLIENTE,
                Type           = MovementType.CREDITO,
                QrCode         = null,
                OriginalAmount = 100m,
                OriginalCurrency = "USD",
                IdempotencyKey = "TX-2026-000001"
            });

        var result = await _paymentHandler.Handle(
            new ExecuteQrPaymentCommand(7, "QR-BO-0001", 20m, "USD", "TX-2026-000001", "Pago"),
            default);

        result.StatusCode.Should().Be(ErrorCode.DUPLICATE_TRANSACTION);
    }

    [Fact]
    public async Task Payment_WhenTheDatabaseReportsAKeyConflict_ReturnsDuplicateTransaction()
    {
        // Carrera: el pre-check no vio nada y la restriccion unica global salto
        // en el motor.
        GivenValidScenario();
        _movementRepository.ExecuteQrPayment(Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.IDEMPOTENCY_CONFLICT);

        var result = await _paymentHandler.Handle(
            new ExecuteQrPaymentCommand(7, "QR-BO-0001", 20m, "USD", "TX-2026-000001", "Pago"),
            default);

        result.StatusCode.Should().Be(ErrorCode.DUPLICATE_TRANSACTION);
    }

    [Fact]
    public async Task ApplyMovement_WhenTheDatabaseReportsAKeyConflict_ReturnsDuplicateTransaction()
    {
        var handler = new ApplyMovementCommandHandler(_accountRepository, _movementRepository);
        _movementRepository.ApplyMovement(Arg.Any<MovementEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.IDEMPOTENCY_CONFLICT);

        var result = await handler.Handle(
            new ApplyMovementCommand("QR0000000001", MovementType.CREDITO, 100m,
                "RECARGA", "TOPUP-1", "Recarga"),
            default);

        result.StatusCode.Should().Be(ErrorCode.DUPLICATE_TRANSACTION);
    }

    private void GivenValidScenario()
    {
        _clientService.GetById(7, Arg.Any<CancellationToken>())
            .Returns(new ClientInfo(7, "Carlos Pérez", "c@mail.com", "PE", "USD", IsActive: true));
        _qrService.GetByCode("QR-BO-0001", Arg.Any<CancellationToken>())
            .Returns(new QrInfo("QR-BO-0001", 55, 139.20m, "BOB", "REF", QrStatus.ACTIVE,
                DateTime.Now.AddHours(1)));
        _merchantService.GetById(55, Arg.Any<CancellationToken>())
            .Returns(new MerchantInfo(55, "Café Central", "123", "BOB", IsActive: true));
        _currencyService.GetByCode(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CurrencyInfo(2, "USD", "$", IsActive: true));
        _currencyService.GetExchangeRate("USD", "BOB", Arg.Any<CancellationToken>()).Returns(6.96m);
        _accountRepository.GetByOwnerAndCoin(AccountOwnerType.CLIENTE, 7, "USD", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity { Number = "QR0000000001", CoinCode = "USD", Balance = 500m, IsActive = true });
        _accountRepository.GetByOwnerAndCoin(AccountOwnerType.COMERCIO, 55, "BOB", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity { Number = "QR0000000002", CoinCode = "BOB", IsActive = true });
    }
}
