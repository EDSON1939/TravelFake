using Core.Domain.Models;
using Country.Domain.Entities;
using Country.Domain.Repositories;
using MediatR;

namespace Country.Application.Features.Countries.Commands.CreateCountry;

public class CreateCountryCommandHandler(ICountryRepository repository)
    : IRequestHandler<CreateCountryCommand, BaseResponse<long>>
{
    public async Task<BaseResponse<long>> Handle(CreateCountryCommand request, CancellationToken ct)
    {
        var id = await repository.Insert(new CountryEntity
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpper()
        }, ct);

        if (id <= 0)
            return BaseResponse<long>.Error(
                Domain.Errors.ErrorCode.INSERT_FAILED,
                Domain.Errors.ErrorMessage.INSERT_FAILED);

        return BaseResponse<long>.Success(id);
    }
}
