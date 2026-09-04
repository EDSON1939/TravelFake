using Commerce.Application.Features.Commerces.Queries.GetCommerceById;
using Commerce.Domain.Entities;
using Commerce.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Commerce.Unit.Tests.Features.Commerces;

public class GetCommerceByIdQueryHandlerTests
{
    private readonly ICommerceRepository         _repository = Substitute.For<ICommerceRepository>();
    private readonly GetCommerceByIdQueryHandler _handler;

    public GetCommerceByIdQueryHandlerTests() => _handler = new GetCommerceByIdQueryHandler(_repository);

    [Fact]
    public async Task Handle_WhenCommerceExists_ReturnsSuccess()
    {
        var entity = new CommerceEntity
        {
            CommerceId = 1, Name = "Tienda Central", Nit = "900123456",
            CuentaId = 5, IsActive = true, CreatedAt = DateTime.UtcNow
        };
        _repository.GetById(1, default).Returns(entity);

        var result = await _handler.Handle(new GetCommerceByIdQuery(1), default);

        result.Should().NotBeNull();
        result.StatusCode.Should().Be(Core.Domain.Errors.ErrorCode.SUC000);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be("Tienda Central");
        result.Data.Nit.Should().Be("900123456");
        result.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCommerceNotFound_ReturnsError()
    {
        _repository.GetById(999, default).Returns((CommerceEntity?)null);

        var result = await _handler.Handle(new GetCommerceByIdQuery(999), default);

        result.StatusCode.Should().Be(Domain.Errors.ErrorCode.COMMERCE_NOT_FOUND);
        result.Data.Should().BeNull();
    }
}
