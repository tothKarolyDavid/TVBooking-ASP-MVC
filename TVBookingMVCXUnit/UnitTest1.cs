using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TVBookingMVCXUnit;

public class BookingAppTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public BookingAppTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Index_ReturnsSuccess()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode);
    }
}

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("TVBOOKING_USE_SQLITE", "true");
        builder.UseEnvironment("Testing");
    }
}