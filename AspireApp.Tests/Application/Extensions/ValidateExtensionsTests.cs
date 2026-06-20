using System.ComponentModel.DataAnnotations;
using System.Net;
using AspireApp.Application.Implementations.Extensions;

namespace AspireApp.Tests.Application.Extensions;

public class ValidateExtensionsTests
{
    private sealed class Sample
    {
        [Required(ErrorMessage = "Name is required.")]
        public string? Name { get; set; }

        [Range(1, 10, ErrorMessage = "Value must be between 1 and 10.")]
        public int Value { get; set; }
    }

    [Fact]
    public void ValidObjectReturnsSameInstance()
    {
        var sample = new Sample { Name = "valid", Value = 5 };

        var result = sample.Validate();

        result.Success.Should().BeTrue();
        result.Value.Should().BeSameAs(sample);
    }

    [Fact]
    public void InvalidObjectReturnsBadRequestWithExpectedError()
    {
        var result = new Sample { Name = null, Value = 5 }.Validate();

        result.IsFailure.Should().BeTrue();
        result.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Errors.Should().Contain("Name is required.");
    }

    [Fact]
    public void MultipleViolationsReturnAllValidationErrors()
    {
        var result = new Sample { Name = null, Value = 20 }.Validate();

        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
