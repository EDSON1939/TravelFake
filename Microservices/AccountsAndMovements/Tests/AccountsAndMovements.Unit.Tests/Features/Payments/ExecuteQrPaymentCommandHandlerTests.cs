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
/// Cubre la lista de pruebas obligatorias del reto para el pago QR: cliente,
/// QR, saldo, moneda, duplicados y concurrencia.
/// </summary>
public class ExecuteQrPaymentCommandHandlerTests
{
    private const string QrCode         = "QR-BO-0001";
    private const string ClientAccount  = "QR0000000001";
    private const string CommerceAccount = "QR0000000002";
    private const long   ClientId       = 7;
    private const long   CommerceId     = 55;
    private const long   QrId           = 1;

    private readonly IAccountRepository  _accountRepository  = Substitute.For<IAccountRepository>();
    private readonly IMovementRepository _movementRepository = Substitute.For<IMovementRepository>();
    private readonly IClientService      _clientService      = Substitute.For<IClientService>();
    private readonly ICommerceService    _commerceService    = Substitute.For<ICommerceService>();
    private readonly IQrService          _qrService          = Substitute.For<IQrService>();
    private readonly ICurrencyService    _currencyService    = Substitute.For<ICurrencyService>();

    private readonly ExecuteQrPaymentCommandHandler _handler;

    public ExecuteQrPaymentCommandHandlerTests()
    {
        _handler = new ExecuteQrPaymentCommandHandler(
            _accountRepository, _movementRepository, _clientService, _commerceService,
            _qrService, _currencyService,
            NullLogger<ExecuteQrPaymentCommandHandler>.Instance);

        // Escenario del reto: Carlos paga 20 USD un QR de 139.20 BOB.
        GivenClient(new ClientInfo(ClientId, "Carlos Pérez", "carlos@mail.com", "PE", "USD", IsActive: true));
        GivenQr(Qr());
        GivenCommerce(new CommerceInfo(CommerceId, "Café Central", "1234567", "BOB", IsActive: true));
        GivenCurrency(new CurrencyInfo(2, "USD", "$", IsActive: true));
        GivenExchangeRate(6.96m);
        GivenAccounts();
        GivenQrConsumed(true);

        _movementRepository.ExecuteQrPayment(Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>())
            .Returns(900L);
        _movementRepository.GetByIdempotencyKey(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((MovementEntity?)null, AppliedMovement());
    }

    // ── Camino feliz ─────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenEverythingIsValid_CompletesThePaymentWithTheConversion()
    {
        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.OriginalAmount.Should().Be(20m);
        result.Data.OriginalCurrency.Should().Be("USD");
        result.Data.ExchangeRate.Should().Be(6.96m);
        result.Data.ConvertedAmount.Should().Be(139.20m);
        result.Data.TargetCurrency.Should().Be("BOB");
        result.Data.IsDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenPaymentIsApplied_SendsTheFrozenConversionToTheDatabase()
    {
        PaymentEntity? captured = null;
        _movementRepository.ExecuteQrPayment(
            Arg.Do<PaymentEntity>(x => captured = x), Arg.Any<CancellationToken>()).Returns(900L);

        await _handler.Handle(Command(), default);

        captured!.ClientAccountNumber.Should().Be(ClientAccount);
        captured.CommerceAccountNumber.Should().Be(CommerceAccount);
        captured.OriginalAmount.Should().Be(20m);
        captured.ExchangeRate.Should().Be(6.96m);
        captured.ConvertedAmount.Should().Be(139.20m);
        captured.TargetCurrency.Should().Be("BOB");
        captured.IdempotencyKey.Should().Be("TX-2026-000001");
        captured.QrCode.Should().Be(QrCode);
    }

    [Fact]
    public async Task Handle_WhenPaymentSucceeds_MarksTheQrAsUsed()
    {
        await _handler.Handle(Command(), default);

        await _qrService.Received(1).Consume(QrCode, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheQrCannotBeMarked_KeepsThePaymentAsValid()
    {
        // El dinero ya se movió: que el servicio de QR no responda no puede
        // convertir un pago aplicado en un error para el cliente.
        GivenQrConsumed(false);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
    }

    // ── Cliente ──────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenClientDoesNotExist_ReturnsCustomerNotFound()
    {
        GivenClient(null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.CUSTOMER_NOT_FOUND);
        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenClientIsInactive_ReturnsCustomerInactive()
    {
        GivenClient(new ClientInfo(ClientId, "Carlos Pérez", "carlos@mail.com", "PE", "USD", IsActive: false));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.CUSTOMER_INACTIVE);
    }

    // ── QR ───────────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenQrDoesNotExist_ReturnsQrNotFound()
    {
        GivenQr(null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenQrIsAlreadyUsed_ReturnsQrAlreadyUsed()
    {
        GivenQr(Qr(status: QrStatus.USED));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_ALREADY_USED);
    }

    [Fact]
    public async Task Handle_WhenQrStatusIsExpired_ReturnsQrExpired()
    {
        GivenQr(Qr(status: QrStatus.EXPIRED));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_EXPIRED);
    }

    [Fact]
    public async Task Handle_WhenQrExpirationHasPassed_ReturnsQrExpired()
    {
        // Sigue ACTIVE en el catálogo, pero la fecha ya venció: manda la fecha,
        // porque el barrido que cambia el estado puede no haber corrido.
        GivenQr(Qr(expiresAt: DateTime.Now.AddMinutes(-1)));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_EXPIRED);
    }

    [Fact]
    public async Task Handle_WhenQrIsDisabled_ReturnsQrInactive()
    {
        // El contrato oficial da de baja un QR con el flag "activo", dejando el
        // estado en ACTIVE: mirar solo el estado dejaria cobrar un QR retirado.
        GivenQr(Qr(isActive: false));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenQrIsMultiUse_ReturnsQrTypeNotSupported()
    {
        // El libro mayor admite un unico DEBITO por codigo de QR
        // (UQ_COMMERCE_MOVIMIENTO_QR), asi que un QR recurrente se rechaza de
        // entrada en vez de cobrarse una vez y mentir en la segunda.
        GivenQr(Qr(type: QrType.MULTIPLE));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_TYPE_NOT_SUPPORTED);
    }

    [Fact]
    public async Task Handle_WhenQrIsMultiUse_DoesNotTouchTheLedger()
    {
        GivenQr(Qr(type: QrType.MULTIPLE));

        await _handler.Handle(Command(), default);

        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    // ── Comercio ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenCommerceDoesNotExist_ReturnsCommerceNotFound()
    {
        GivenCommerce(null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.COMMERCE_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenCommerceIsInactive_ReturnsCommerceInactive()
    {
        GivenCommerce(new CommerceInfo(CommerceId, "Café Central", "1234567", "BOB", IsActive: false));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.COMMERCE_INACTIVE);
    }

    // ── Moneda y conversión ──────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenCurrencyIsNotInTheCatalog_ReturnsCurrencyNotSupported()
    {
        GivenCurrency(null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.CURRENCY_NOT_SUPPORTED);
    }

    [Fact]
    public async Task Handle_WhenCurrencyIsInactive_ReturnsCurrencyNotSupported()
    {
        GivenCurrency(new CurrencyInfo(2, "USD", "$", IsActive: false));

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.CURRENCY_NOT_SUPPORTED);
    }

    [Fact]
    public async Task Handle_WhenThereIsNoExchangeRate_ReturnsExchangeRateNotFound()
    {
        _currencyService.GetExchangeRate("USD", "BOB", Arg.Any<CancellationToken>())
            .Returns((decimal?)null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.EXCHANGE_RATE_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenConvertedAmountDoesNotCoverTheQr_ReturnsInvalidAmount()
    {
        // 10 USD x 6.96 = 69.60 BOB, y el QR cobra 139.20.
        var result = await _handler.Handle(Command(amount: 10m), default);

        result.StatusCode.Should().Be(ErrorCode.INVALID_AMOUNT);
        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    // ── Cuentas y saldo ──────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenClientHasNoAccountInThatCurrency_ReturnsAccountNotFound()
    {
        _accountRepository.GetByHolderAndCoin(
            AccountType.CLIENT, ClientId, "USD", Arg.Any<CancellationToken>())
            .Returns((AccountEntity?)null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenCommerceHasNoAccount_ReturnsCommerceAccountNotFound()
    {
        _accountRepository.GetByHolderAndCoin(
            AccountType.COMMERCE, CommerceId, "BOB", Arg.Any<CancellationToken>())
            .Returns((AccountEntity?)null);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.COMMERCE_ACCOUNT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenTheDatabaseReportsInsufficientFunds_ReturnsInsufficientFunds()
    {
        _movementRepository.ExecuteQrPayment(Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.INSUFFICIENT_FUNDS);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.INSUFFICIENT_FUNDS);
        result.Data.Should().BeNull();
        // Un pago que no ocurrió no puede consumir el QR.
        await _qrService.DidNotReceive().Consume(
            Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheDatabaseReportsTheQrWasUsed_ReturnsQrAlreadyUsed()
    {
        // Carrera perdida contra otro cliente que pagó el mismo QR primero.
        _movementRepository.ExecuteQrPayment(Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>())
            .Returns(PaymentResult.QR_ALREADY_USED);

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(ErrorCode.QR_ALREADY_USED);
    }

    // ── Idempotencia ─────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenTheKeyWasAlreadyUsedWithTheSameData_ReturnsTheOriginalWithoutCharging()
    {
        _movementRepository.GetByIdempotencyKey("TX-2026-000001", Arg.Any<CancellationToken>())
            .Returns(AppliedMovement());

        var result = await _handler.Handle(Command(), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data!.IsDuplicate.Should().BeTrue();
        result.Data.MovementId.Should().Be(900L);
        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheKeyIsReusedWithOtherData_ReturnsDuplicateTransaction()
    {
        _movementRepository.GetByIdempotencyKey("TX-2026-000001", Arg.Any<CancellationToken>())
            .Returns(AppliedMovement());

        // Misma clave, otro monto: no es un reintento, es una clave reusada.
        var result = await _handler.Handle(Command(amount: 35m), default);

        result.StatusCode.Should().Be(ErrorCode.DUPLICATE_TRANSACTION);
        await _movementRepository.DidNotReceive().ExecuteQrPayment(
            Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>());
    }

    // ── Concurrencia ─────────────────────────────────────────────────────────
    [Fact]
    public async Task Handle_WhenTwoPaymentsRaceOverTheSameBalance_OnlyOneIsApproved()
    {
        // Escenario del reto: saldo 100, llegan 80 y 50 a la vez. El SP resuelve
        // con la fila bloqueada; aquí se simula ese comportamiento serializado
        // para verificar que el handler traduce bien el rechazo del segundo.
        GivenLocalClientPayingInBob(balance: 100m);

        var first  = _handler.Handle(BobCommand(80m, "TX-A"), default);
        var second = _handler.Handle(BobCommand(50m, "TX-B"), default);

        var results = await Task.WhenAll(first, second);

        results.Count(r => r.StatusCode == Core.Domain.Errors.ErrorCode.SUC000).Should().Be(1);
        results.Count(r => r.StatusCode == ErrorCode.INSUFFICIENT_FUNDS).Should().Be(1);
        _simulatedBalance.Should().BeGreaterThanOrEqualTo(0m);
    }

    // ── Arreglo del escenario ────────────────────────────────────────────────
    private decimal _simulatedBalance;
    private long    _simulatedMovementId;

    private static ExecuteQrPaymentCommand Command(
        decimal amount = 20m, string key = "TX-2026-000001")
        => new(ClientId, QrCode, amount, "USD", key, "Pago QR en comercio");

    private static QrInfo Qr(
        string status     = QrStatus.ACTIVE,
        DateTime? expiresAt = null,
        decimal amount    = 139.20m,
        bool isActive     = true,
        string type       = QrType.UNICO)
        => new(QrId, QrCode, CommerceId, amount, type, status, isActive, expiresAt ?? DateTime.Now.AddHours(1));

    private static MovementEntity AppliedMovement() => new()
    {
        MovementId       = 900L,
        AccountNumber    = ClientAccount,
        AccountType      = AccountType.CLIENT,
        HolderId         = ClientId,
        Type             = MovementType.DEBITO,
        Status           = MovementStatus.COMPLETED,
        Amount           = 20m,
        BalanceBefore    = 500m,
        BalanceAfter     = 480m,
        OriginalAmount   = 20m,
        OriginalCurrency = "USD",
        ExchangeRate     = 6.96m,
        ConvertedAmount  = 139.20m,
        TargetCurrency   = "BOB",
        CommerceId       = CommerceId,
        QrCode           = QrCode,
        Reference        = "QR-1",
        IdempotencyKey   = "TX-2026-000001",
        TransactionCode  = "d3f1b6f0-0000-4000-8000-000000000001",
        CreatedAt        = DateTime.Now
    };

    private void GivenClient(ClientInfo? client)
        => _clientService.GetById(ClientId, Arg.Any<CancellationToken>()).Returns(client);

    private void GivenQr(QrInfo? qr)
        => _qrService.GetByCode(QrCode, Arg.Any<CancellationToken>()).Returns(qr);

    private void GivenCommerce(CommerceInfo? commerce)
        => _commerceService.GetById(CommerceId, Arg.Any<CancellationToken>()).Returns(commerce);

    private void GivenCurrency(CurrencyInfo? currency)
        => _currencyService.GetByCode(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(currency);

    private void GivenExchangeRate(decimal rate)
        => _currencyService.GetExchangeRate("USD", "BOB", Arg.Any<CancellationToken>()).Returns(rate);

    private void GivenQrConsumed(bool consumed)
        => _qrService.Consume(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(consumed);

    private void GivenAccounts()
    {
        _accountRepository.GetByHolderAndCoin(
            AccountType.CLIENT, ClientId, "USD", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity
            {
                AccountId = 1, Number = ClientAccount, AccountType = AccountType.CLIENT,
                HolderId = ClientId, CoinId = 2, CoinCode = "USD", Balance = 500m, IsActive = true
            });

        _accountRepository.GetByHolderAndCoin(
            AccountType.COMMERCE, CommerceId, "BOB", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity
            {
                AccountId = 2, Number = CommerceAccount, AccountType = AccountType.COMMERCE,
                HolderId = CommerceId, CoinId = 1, CoinCode = "BOB", Balance = 0m, IsActive = true
            });
    }

    /// <summary>
    /// Cliente que paga directo en BOB con un QR de monto abierto, y un
    /// repositorio que simula el bloqueo de fila del SP: los pagos se resuelven
    /// de a uno y el saldo nunca queda negativo.
    /// </summary>
    private void GivenLocalClientPayingInBob(decimal balance)
    {
        _simulatedBalance = balance;

        // En este escenario ninguna clave existe todavia: sin esto, el stub
        // secuencial del constructor haria pasar al segundo pago por reintento.
        _movementRepository.GetByIdempotencyKey(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((MovementEntity?)null);

        GivenClient(new ClientInfo(ClientId, "Carlos Pérez", "carlos@mail.com", "BO", "BOB", IsActive: true));
        GivenQr(Qr(amount: 0m));
        GivenCurrency(new CurrencyInfo(1, "BOB", "Bs", IsActive: true));

        _accountRepository.GetByHolderAndCoin(
            AccountType.CLIENT, ClientId, "BOB", Arg.Any<CancellationToken>())
            .Returns(new AccountEntity
            {
                AccountId = 1, Number = ClientAccount, AccountType = AccountType.CLIENT,
                HolderId = ClientId, CoinId = 1, CoinCode = "BOB", Balance = balance, IsActive = true
            });

        var gate = new object();
        _movementRepository.ExecuteQrPayment(Arg.Any<PaymentEntity>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var payment = call.Arg<PaymentEntity>();
                lock (gate)
                {
                    if (_simulatedBalance < payment.OriginalAmount)
                        return PaymentResult.INSUFFICIENT_FUNDS;

                    _simulatedBalance -= payment.OriginalAmount;
                    return ++_simulatedMovementId;
                }
            });
    }

    private static ExecuteQrPaymentCommand BobCommand(decimal amount, string key)
        => new(ClientId, QrCode, amount, "BOB", key, "Pago QR en comercio");
}
