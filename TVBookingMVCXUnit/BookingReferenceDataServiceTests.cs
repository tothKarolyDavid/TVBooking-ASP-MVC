using TVBookingMVC.Services;

namespace TVBookingMVCXUnit;

public class BookingReferenceDataServiceTests
{
    [Fact]
    public void AgeLimits_ContainsExpectedEntries()
    {
        var service = new BookingReferenceDataService();

        Assert.NotEmpty(service.AgeLimits);
        Assert.Contains("Gyermekbarát program", service.AgeLimits);
        Assert.Contains("18 éven aluliak számára nem ajánlott", service.AgeLimits);
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
        Assert.Contains("Akció", service.Genres);
        Assert.Contains("Dokumentum", service.Genres);
        Assert.Contains("Sport", service.Genres);
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