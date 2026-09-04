using AutoMapper;
using Commerce.Api.Grpc;
using Commerce.Application.Features.Commerces.Commands.CreateCommerce;
using Commerce.Application.Features.Commerces.Commands.UpdateCommerce;
using Commerce.Application.Features.Commerces.Commands.UpdateStatus;
using Commerce.Application.Features.Commerces.Queries.GetCommerceById;
using Commerce.Application.Features.Commerces.Queries.GetCommerceList;
using Grpc.Core;
using MediatR;

namespace Commerce.Api.Services;

public class CommerceService(ISender sender, IMapper mapper) : Commerce.Api.Grpc.Commerce.CommerceBase
{
    public override async Task<CommerceMutationBaseResponsePb> CreateCommerce(
        CreateCommerceRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new CreateCommerceCommand(request.Name, request.Nit),
            context.CancellationToken);

        return mapper.Map<CommerceMutationBaseResponsePb>(result);
    }

    public override async Task<GetCommerceBaseResponsePb> GetCommerceById(
        GetCommerceByIdRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new GetCommerceByIdQuery(request.Id),
            context.CancellationToken);

        return mapper.Map<GetCommerceBaseResponsePb>(result);
    }

    public override async Task<GetCommerceListBaseResponsePb> GetCommerceList(
        GetCommerceListRequestPb request, ServerCallContext context)
    {
        var pageNumber = request.HasPageNumber ? request.PageNumber : 1;
        var pageSize = request.HasPageSize ? request.PageSize : 10;

        var result = await sender.Send(
            new GetCommerceListQuery(request.Name, request.OnlyActive, pageNumber, pageSize),
            context.CancellationToken);

        return mapper.Map<GetCommerceListBaseResponsePb>(result);
    }

    public override async Task<CommerceMutationBaseResponsePb> UpdateCommerce(
        UpdateCommerceRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCommerceCommand(request.Id, request.Name, request.Nit),
            context.CancellationToken);

        return mapper.Map<CommerceMutationBaseResponsePb>(result);
    }

    public override async Task<CommerceMutationBaseResponsePb> UpdateCommerceStatus(
        UpdateCommerceStatusRequestPb request, ServerCallContext context)
    {
        var result = await sender.Send(
            new UpdateCommerceStatusCommand(request.Id, request.IsActive),
            context.CancellationToken);

        return mapper.Map<CommerceMutationBaseResponsePb>(result);
    }
}
