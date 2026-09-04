using Core.Domain.Models;
using MediatR;

namespace Country.Application.Features.Countries.Commands.UpdateCountry;

public record UpdateCountryCommand(long CountryId, string Name, string Code, bool IsActive)
    : IRequest<BaseResponse<long>>;
