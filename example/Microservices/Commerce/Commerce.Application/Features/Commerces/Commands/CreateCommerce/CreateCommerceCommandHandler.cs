using Commerce.Domain.Interfaces;
using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.CreateCommerce;

public class CreateCommerceCommandHandler(
    ICommerceRepository repository,
    IAccountsAndMovementsService accountsAndMovementsService)
    : IRequestHandler<CreateCommerceCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateCommerceCommand request, CancellationToken ct)
    {
        var duplicate = await repository.ExistsByNit(request.Nit.Trim(), ct);
        if (duplicate)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_DUPLICATE,
                Domain.Errors.ErrorMessage.COMMERCE_DUPLICATE);

        var id = await repository.Insert(new Domain.Entities.CommerceEntity
        {
            Name     = request.Name.Trim(),
            Nit      = request.Nit.Trim(),
            IsActive = true
        }, ct);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        var account = await accountsAndMovementsService.CreateCommerceAccount(id, "0", ct);
        if (!account.IsSuccess())
            return BaseResponse<long>.Error(
                account.StatusCode,
                account.Message ?? Domain.Errors.ErrorMessage.ACCOUNT_CREATE_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
