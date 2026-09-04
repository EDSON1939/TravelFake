using Core.Domain.Models;
using MediatR;

namespace Country.Application.Features.Countries.Commands.CreateCountry;

public record CreateCountryCommand(string Name, string Code)
    : IRequest<BaseResponse<long>>;
