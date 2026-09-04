using Core.Domain.Models;
using Country.Application.Common;
using MediatR;

namespace Country.Application.Features.Countries.Queries.GetCountry;

public record GetCountryQuery(long CountryId) : IRequest<BaseResponse<CountryResponse>>;
