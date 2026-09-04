using AutoMapper;
using Client.Api.Grpc;
using Client.Application.Features.Clients.Commands.CreateClient;
using Client.Application.Features.Clients.Commands.DeleteClient;
using Client.Application.Features.Clients.Commands.UpdateClient;
using Client.Application.Features.Clients.Commands.UpdateClientStatus;
using Client.Application.Features.Clients.Queries.GetClient;
using Client.Application.Features.Clients.Queries.GetClients;
using Client.Application.Features.Countries.Queries.GetCountries;
using Grpc.Core;
using MediatR;

namespace Client.Api.Services;

public class ClientService(ISender sender, IMapper mapper)
    : global::Client.Api.Grpc.Client.ClientBase
{
    public override async Task<GetClientBaseResponsePb> GetClient(
        GetClientRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetClientQuery(request.CustomerId),
            context.CancellationToken);

        return mapper.Map<GetClientBaseResponsePb>(result);
    }

    public override async Task<GetClientsBaseResponsePb> GetClients(
        GetClientsRequestPb request, ServerCallContext context)
    {
        // country_id = 0 significa "todos los países" (el SP espera NULL)
        var result = await sender.Send(
            new GetClientsQuery(request.OnlyActive, request.CountryId > 0 ? request.CountryId : null),
            context.CancellationToken);

        return mapper.Map<GetClientsBaseResponsePb>(result);
    }

    public override async Task<ClientMutationBaseResponsePb> CreateClient(
        CreateClientRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateClientCommand(
                request.FirstName, request.LastName,
                request.Email, request.Phone, request.CountryId),
            context.CancellationToken);

        return mapper.Map<ClientMutationBaseResponsePb>(result);
    }

    public override async Task<ClientMutationBaseResponsePb> UpdateClient(
        UpdateClientRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateClientCommand(
                request.CustomerId, request.FirstName, request.LastName,
                request.Email, request.Phone, request.CountryId, request.IsActive),
            context.CancellationToken);

        return mapper.Map<ClientMutationBaseResponsePb>(result);
    }

    public override async Task<ClientMutationBaseResponsePb> UpdateClientStatus(
        UpdateClientStatusRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateClientStatusCommand(request.CustomerId, request.IsActive),
            context.CancellationToken);

        return mapper.Map<ClientMutationBaseResponsePb>(result);
    }

    public override async Task<ClientMutationBaseResponsePb> DeleteClient(
        DeleteClientRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new DeleteClientCommand(request.CustomerId),
            context.CancellationToken);

        return mapper.Map<ClientMutationBaseResponsePb>(result);
    }

    /// <summary>
    /// No toca la base de datos de clientes: delega en el microservicio TravelCountry.
    /// </summary>
    public override async Task<GetCountriesBaseResponsePb> GetCountries(
        GetCountriesRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCountriesQuery(request.OnlyActive),
            context.CancellationToken);

        return mapper.Map<GetCountriesBaseResponsePb>(result);
    }
}
