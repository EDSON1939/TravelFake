using Core.Domain.Models;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Commands.UpdateCountryStatus;

public class UpdateCountryStatusCommandHandler(ICountryRepository repository)
    : IRequestHandler<UpdateCountryStatusCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(UpdateCountryStatusCommand request, CancellationToken ct)
    {
        var affected = await repository.UpdateStatus(request.CountryId, request.IsActive, ct);

        if (affected <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.COUNTRY_NOT_FOUND,
                Domain.Errors.ErrorMessage.COUNTRY_NOT_FOUND);

        return BaseResponse<long>.Success(request.CountryId);
    }
}
