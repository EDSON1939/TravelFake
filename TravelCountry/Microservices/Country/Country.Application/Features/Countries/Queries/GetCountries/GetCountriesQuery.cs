using Core.Domain.Models;
using Country.Application.Common;
using MediatR;

namespace Country.Application.Features.Countries.Queries.GetCountries;

public record GetCountriesQuery(bool OnlyActive = true) : IRequest<BaseResponse<List<CountryResponse>>>;
