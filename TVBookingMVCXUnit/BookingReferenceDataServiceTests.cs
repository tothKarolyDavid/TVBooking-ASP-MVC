using TVBookingMVC.Services;

namespace TVBookingMVCXUnit;

public class BookingReferenceDataServiceTests
{
    [Fact]
    public void AgeLimits_ContainsExpectedEntries()
    {
        var service = new BookingReferenceDataService();

        Assert.NotEmpty(service.AgeLimits);
        Assert.Contains("Child-friendly Program", service.AgeLimits);
        Assert.Contains("Under 18 not recommended", service.AgeLimits);
    }

    [Fact]
    public void Channels_ContainsExpectedEntries()
    {
        var service = new BookingReferenceDataService();

        Assert.NotEmpty(service.Channels);
        Assert.Contains("M1", service.Channels);
        Assert.Contains("HBO", service.Channels);
        Assert.Contains("RTL", service.Channels);
    }

    [Fact]
    public void Genres_ContainsExpectedEntries()
    {
        var service = new BookingReferenceDataService();

        Assert.NotEmpty(service.Genres);
        Assert.Contains("Action", service.Genres);
        Assert.Contains("Documentary", service.Genres);
        Assert.Contains("Sports", service.Genres);
    }

    [Fact]
    public void AgeLimits_AreReadOnlyCollections()
    {
        var service = new BookingReferenceDataService();

        Assert.IsAssignableFrom<IReadOnlyList<string>>(service.AgeLimits);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(service.Channels);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(service.Genres);
    }
}