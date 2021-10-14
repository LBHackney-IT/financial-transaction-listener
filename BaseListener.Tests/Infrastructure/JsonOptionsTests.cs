using BaseListener.Infrastructure;
using FluentAssertions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace BaseListener.Tests.Infrastructure
{
    public class JsonOptionsTests
    {
        [Fact]
        public void CreateJsonOptionsTest()
        {
            var options = JsonOptions.CreateJsonOptions();

            options.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
            options.WriteIndented.Should().BeTrue();
            options.Converters.Should().ContainEquivalentOf(new JsonStringEnumConverter());
        }
    }
}
