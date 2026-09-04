using AccountsAndMovements.Application.Features.Accounts.Commands.CreateMerchantAccount;
using AccountsAndMovements.Domain.Entities;
using AccountsAndMovements.Domain.ExternalServices;
using AccountsAndMovements.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;
using ErrorCode = AccountsAndMovements.Domain.Errors.ErrorCode;

namespace AccountsAndMovements.Unit.Tests.Features.Accounts;

public class CreateMerchantAccountCommandHandlerTests
{
    private readonly IAccountRepository _repository      = Substitute.For<IAccountRepository>();
    private readonly IMerchantService   _merchantService = Substitute.For<IMerchantService>();
    private readonly ICurrencyService   _currencyService = Substitute.For<ICurrencyService>();

    private readonly CreateMerchantAccountCommandHandler _handler;

    public CreateMerchantAccountCommandHandlerTests()
    {
        _handler = new CreateMerchantAccountCommandHandler(
            _repository, _merchantService, _currencyService);

        _merchantService.GetById(55, Arg.Any<CancellationToken>())
            .Returns(new MerchantInfo(55, "Café Central", "1234567", "BOB", IsActive: true));
        _currencyService.GetByCode(CurrencyCode.BOB, Arg.Any<CancellationToken>())
            .Returns(new CurrencyInfo(1, "BOB", "Bs", IsActive: true));
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>()).Returns(43L);
    }

    [Fact]
    public async Task Handle_WhenMerchantIsValid_CreatesTheAccount()
    {
        var result = await _handler.Handle(new CreateMerchantAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().Be(43L);
    }

    [Fact]
    public async Task Handle_AlwaysCreatesTheAccountInBob()
    {
        // La moneda no es un parametro: el contrato ya no permite pedir otra, y
        // por eso MERCHANT_CURRENCY_INVALID dejo de poder ocurrir.
        AccountEntity? captured = null;
        _repository.Insert(Arg.Do<AccountEntity>(x => captured = x), Arg.Any<CancellationToken>())
            .Returns(43L);

        await _handler.Handle(new CreateMerchantAccountCommand(55, 0m), default);

        captured!.OwnerType.Should().Be(AccountOwnerType.COMERCIO);
        captured.OwnerId.Should().Be(55);
        captured.CoinCode.Should().Be(CurrencyCode.BOB);
        captured.CoinId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenMerchantDoesNotExist_ReturnsMerchantNotFound()
    {
        _merchantService.GetById(99, Arg.Any<CancellationToken>()).Returns((MerchantInfo?)null);

        var result = await _handler.Handle(new CreateMerchantAccountCommand(99, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.MERCHANT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_WhenMerchantIsInactive_ReturnsMerchantInactive()
    {
        _merchantService.GetById(55, Arg.Any<CancellationToken>())
            .Returns(new MerchantInfo(55, "Café Central", "1234567", "BOB", IsActive: false));

        var result = await _handler.Handle(new CreateMerchantAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.MERCHANT_INACTIVE);
    }

    [Fact]
    public async Task Handle_WhenTheMerchantAlreadyHasABobAccount_ReturnsDuplicate()
    {
        _repository.Insert(Arg.Any<AccountEntity>(), Arg.Any<CancellationToken>())
            .Returns(AccountResult.DUPLICATE);

        var result = await _handler.Handle(new CreateMerchantAccountCommand(55, 0m), default);

        result.StatusCode.Should().Be(ErrorCode.ACCOUNT_DUPLICATE);
    }
}
