using AccountsAndMovements.Application.Features.Accounts.Commands.CreateClientAccount;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Accounts;

public class CreateClientAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository      = Substitute.For<IAccountRepository>();
    private readonly IClientService     _clientService   = Substitute.For<IClientService>();
    private readonly ICurrencyService   _currencyService = Substitute.For<ICurrencyService>();

    private readonly CreateClientAccountCommandHandler _handler;

    public CreateClientAccountCommandHandlerTests()
    {
        _handler = new CreateClientAccountCommandHandler(
            _repository, _clientService, _currencyService);

        _clientService.GetById(7, Arg.Any<CancellationToken>())
            .Returns(new ClientInfo(7, "Carlos Pérez", "carlos@mail.com", "PE", "PEN", IsActive: true));
        _currencyService.GetByCode(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new CurrencyInfo(3, "PEN", "S/", IsActive: true));
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>()).Returns(42L);
    }

    [Fact]
    public async Task Handle_WhenClientAndCurrencyAreValid_CreatesTheAccount()
    {
        var result = await _handler.Handle(new CreateClientAccountCommand(7, "PEN", 2500m), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(42L);
    }

    [Fact]
    public async Task Handle_PassesTheResolvedCurrencyAndOpeningBalanceToTheRepository()
    {
        AccountEntity? captured = null;
        _repository.Insert(Arg.Do<AccountEntity>(x => captured = x), Arg.Any<CancellationToken>())
            .Returns(42L);

        await _handler.Handle(new CreateClientAccountCommand(7, " pen ", 2500m), default);

        // El tipo de titular ya no viene del request: lo fija el propio comando.
        captured!.AccountType.Should().Be(AccountType.CLIENT);
        captured.HolderId.Should().Be(7);
        captured.CoinId.Should().Be(3);
        captured.CoinCode.Should().Be("PEN");
        captured.Balance.Should().Be(2500m);
    }

    [Fact]
    public async Task Handle_WhenClientDoesNotExist_ReturnsCustomerNotFound()
    {
        _clientService.GetById(99, Arg.Any<CancellationToken>()).Returns((ClientInfo?)null);

        var result = await _handler.Handle(new CreateClientAccountCommand(99, "PEN", 0m), default);

        result.StatusCode.Should().Be(ErrorCode.CUSTOMER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenClientIsInactive_ReturnsCustomerInactive()
    {
        _clientService.GetById(7, Arg.Any<CancellationToken>())
            .Returns(new ClientInfo(7, "Carlos Pérez", "carlos@mail.com", "PE", "PEN", IsActive: false));

        var result = await _handler.Handle(new CreateClientAccountCommand(7, "PEN", 0m), default);

        result.StatusCode.Should().Be(ErrorCode.CUSTOMER_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenCurrencyIsNotSupported_ReturnsCurrencyNotSupported()
    {
        _currencyService.GetByCode(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((CurrencyInfo?)null);

        var result = await _handler.Handle(new CreateClientAccountCommand(7, "XXX", 0m), default);

        result.StatusCode.Should().Be(ErrorCode.CURRENCY_NOT_SUPPORTED);
    }

    [Fact]
    public async Task Handle_WhenTheClientAlreadyHasAnAccountInThatCurrency_ReturnsDuplicate()
    {
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
            .Returns(AccountResult.DUPLICATE);

        var result = await _handler.Handle(new CreateClientAccountCommand(7, "PEN", 0m), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_DUPLICATE);
    }
}
