using Core.Domain.Models;
using MediatR;

namespace Country.Application.Features.Countries.Commands.DeleteCountry;

public record DeleteCountryCommand(long CountryId) : IRequest<BaseResponse<long>>;
