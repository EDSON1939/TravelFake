using AutoMapper;
using Country.Api.Grpc;
using Country.Application.Features.Countries.Commands.CreateCountry;
using Country.Application.Features.Countries.Commands.DeleteCountry;
using Country.Application.Features.Countries.Commands.UpdateCountry;
using Country.Application.Features.Countries.Commands.UpdateCountryStatus;
using Country.Application.Features.Countries.Queries.GetCountries;
using Country.Application.Features.Countries.Queries.GetCountry;
using Grpc.Core;
using MediatR;

namespace Country.Api.Services;

public class CountryService(ISender sender, IMapper mapper)
    : global::Country.Api.Grpc.Country.CountryBase
{
    public override async Task<GetCountryBaseResponsePb> GetCountry(
        GetCountryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCountryQuery(request.CountryId),
            context.CancellationToken);

        return mapper.Map<GetCountryBaseResponsePb>(result);
    }

    public override async Task<GetCountriesBaseResponsePb> GetCountries(
        GetCountriesRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCountriesQuery(request.OnlyActive),
            context.CancellationToken);

        return mapper.Map<GetCountriesBaseResponsePb>(result);
    }

    public override async Task<CountryMutationBaseResponsePb> CreateCountry(
        CreateCountryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateCountryCommand(request.Name, request.Code),
            context.CancellationToken);

        return mapper.Map<CountryMutationBaseResponsePb>(result);
    }

    public override async Task<CountryMutationBaseResponsePb> UpdateCountry(
        UpdateCountryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCountryCommand(request.CountryId, request.Name, request.Code, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CountryMutationBaseResponsePb>(result);
    }

    public override async Task<CountryMutationBaseResponsePb> UpdateCountryStatus(
        UpdateCountryStatusRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCountryStatusCommand(request.CountryId, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CountryMutationBaseResponsePb>(result);
    }

    public override async Task<CountryMutationBaseResponsePb> DeleteCountry(
        DeleteCountryRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new DeleteCountryCommand(request.CountryId),
            context.CancellationToken);

        return mapper.Map<CountryMutationBaseResponsePb>(result);
    }
}
