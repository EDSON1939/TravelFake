using Client.Application.Common;
using Core.Domain.Models;
using MediatR;

namespace Client.Application.Features.Countries.Queries.GetCountries;

public record GetCountriesQuery(bool OnlyActive) : IRequest<BaseResponse<List<CountryResponse>>>;
