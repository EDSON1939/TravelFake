using Commerce.Domain.Repositories;
using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.CreateCommerce;

public class CreateCommerceCommandHandler(ICommerceRepository repository)
    : IRequestHandler<CreateCommerceCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateCommerceCommand request, CancellationToken ct)
    {
        var duplicate = await repository.ExistsByNit(request.Nit.Trim(), ct);
        if (duplicate)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COMMERCE_DUPLICATE,
                Domain.Errors.ErrorMessage.COMMERCE_DUPLICATE);

        // TODO: Crear/asignar CuentaId para el comercio (no implementado aún).
        var id = await repository.Insert(new Domain.Entities.CommerceEntity
        {
            Name     = request.Name.Trim(),
            Nit      = request.Nit.Trim(),
            CuentaId = null,
            IsActive = true
        }, ct);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
