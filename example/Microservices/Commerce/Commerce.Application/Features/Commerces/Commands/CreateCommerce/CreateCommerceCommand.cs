using Core.Domain.Models;
using MediatR;

namespace Commerce.Application.Features.Commerces.Commands.CreateCommerce;

public record CreateCommerceCommand(string Name, string Nit)
    : IRequest<BaseResponse<long>>;
