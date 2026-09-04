using Core.Domain.Models;
using MediatR;

namespace Country.Application.Features.Countries.Commands.UpdateCountryStatus;

public record UpdateCountryStatusCommand(long CountryId, bool IsActive)
    : IRequest<BaseResponse<long>>;
