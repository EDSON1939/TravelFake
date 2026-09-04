using Commerce.Application.Features.Commerces.Queries.GetCommerceById;
using FluentAssertions;
using Xunit;

namespace Commerce.Unit.Tests.Features.Commerces;

public class GetCommerceByIdQueryValidatorTests
{
    private readonly GetCommerceByIdQueryValidator _validator = new();

    [Fact]
    public async Task Validate_WhenIdIsPositive_PassesValidation()
    {
        var result = await _validator.ValidateAsync(new GetCommerceByIdQuery(1));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Validate_WhenIdIsNotPositive_FailsValidation(long id)
    {
        var result = await _validator.ValidateAsync(new GetCommerceByIdQuery(id));
        result.IsValid.Should().BeFalse();
    }
}
