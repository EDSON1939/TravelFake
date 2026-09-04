using AccountsAndMovements.Application.Features.Accounts.Commands.CreateCommerceAccount;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Accounts;

public class CreateCommerceAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository      = Substitute.For<IAccountRepository>();
    private readonly ICommerceService   _commerceService = Substitute.For<ICommerceService>();
    private readonly ICurrencyService   _currencyService = Substitute.For<ICurrencyService>();

    private readonly CreateCommerceAccountCommandHandler _handler;

    public CreateCommerceAccountCommandHandlerTests()
    {
        _handler = new CreateCommerceAccountCommandHandler(
            _repository, _commerceService, _currencyService);

        _commerceService.GetById(55, Arg.Any<CancellationToken>())
            .Returns(new CommerceInfo(55, "Café Central", "1234567", "BOB", IsActive: true));
        _currencyService.GetByCode(CurrencyCode.BOB, Arg.Any<CancellationToken>())
            .Returns(new CurrencyInfo(1, "BOB", "Bs", IsActive: true));
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>()).Returns(43L);
    }

    [Fact]
    public async Task Handle_WhenCommerceIsValid_CreatesTheAccount()
    {
        var result = await _handler.Handle(new CreateCommerceAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(43L);
    }

    [Fact]
    public async Task Handle_AlwaysCreatesTheAccountInBob()
    {
        // La moneda no es un parametro: el contrato ya no permite pedir otra, y
        // por eso COMMERCE_CURRENCY_INVALID dejo de poder ocurrir.
        AccountEntity? captured = null;
        _repository.Insert(Arg.Do<AccountEntity>(x => captured = x), Arg.Any<CancellationToken>())
            .Returns(43L);

        await _handler.Handle(new CreateCommerceAccountCommand(55, 0m), default);

        captured!.AccountType.Should().Be(AccountType.COMMERCE);
        captured.HolderId.Should().Be(55);
        captured.CoinCode.Should().Be(CurrencyCode.BOB);
        captured.CoinId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenCommerceDoesNotExist_ReturnsCommerceNotFound()
    {
        _commerceService.GetById(99, Arg.Any<CancellationToken>()).Returns((CommerceInfo?)null);

        var result = await _handler.Handle(new CreateCommerceAccountCommand(99, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.COMMERCE_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenCommerceIsInactive_ReturnsCommerceInactive()
    {
        _commerceService.GetById(55, Arg.Any<CancellationToken>())
            .Returns(new CommerceInfo(55, "Café Central", "1234567", "BOB", IsActive: false));

        var result = await _handler.Handle(new CreateCommerceAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.COMMERCE_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenTheCommerceAlreadyHasABobAccount_ReturnsDuplicate()
    {
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
            .Returns(AccountResult.DUPLICATE);

        var result = await _handler.Handle(new CreateCommerceAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_DUPLICATE);
    }
}
