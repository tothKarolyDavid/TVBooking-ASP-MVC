using TVBookingMVC.Models;
using TVBookingMVC.Services;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingReferenceDataServiceTests
{
    private readonly BookingReferenceDataService _service = new();

    public static readonly string[] ExpectedAgeLimits =
    [
        "Child-friendly Program",
        "General Audience",
        "Under 6 not recommended",
        "Under 12 not recommended",
        "Under 16 not recommended",
        "Under 18 not recommended",
        "Adults Only"
    ];

    public static readonly string[] ExpectedChannels =
    [
        "M1",
        "M2",
        "M4 Sport",
        "M5",
        "Duna",
        "Duna World",
        "ATV",
        "ATV Spirit",
        "AXN",
        "Cartoon Network",
        "Comedy Central",
        "FEM3",
        "FIXHD (Kizárólag online)",
        "HBO",
        "Hír TV",
        "Izaura TV",
        "Minimax",
        "Moziverzum",
        "Prime",
        "RTL",
        "RTL Gold",
        "RTL Kettő",
        "Sláger TV",
        "Spektrum",
        "Spíler TV",
        "Super TV2",
        "TV2",
        "TV2 Comedy",
        "TV2 Kids",
        "TV2 Séf",
        "VIASAT2",
        "VIASAT3",
        "VIASAT6",
        "Zenebutik",
        "Disney Channel",
        "Nickelodeon",
        "Nick Jr.",
        "NickToons",
        "TeenNick",
        "Paramount Network",
        "National Geographic",
        "National Geographic Wild",
        "Discovery Channel",
        "Animal Planet",
        "Discovery Turbo Extra",
        "Discovery Science",
        "TLC",
        "Investigation Discovery",
        "Viasat Explore",
        "Viasat History",
        "Viasat Nature",
        "Da Vinci",
        "History",
        "Fishing and Hunting",
        "Eurosport 1",
        "Eurosport 2"
    ];

    public static readonly string[] ExpectedGenres =
    [
        "Action",
        "Animation",
        "Family",
        "Documentary",
        "Drama",
        "Lifestyle",
        "Fantasy",
        "Movie",
        "Children",
        "News",
        "Horror",
        "Educational",
        "Game Show",
        "Adventure",
        "Comic",
        "Comedy",
        "Concert",
        "Crime",
        "Fairy Tale",
        "Animated Film",
        "Reality",
        "Romantic",
        "Sci-fi",
        "Show",
        "Sitcom",
        "Sports",
        "Entertainment",
        "Talk Show",
        "Other"
    ];

    [Fact]
    public void AgeLimits_ReturnsAllExpectedEntries()
    {
        Assert.Equal(ExpectedAgeLimits, _service.AgeLimits);
    }

    [Fact]
    public void AgeLimits_HasNoDuplicates()
    {
        Assert.Equal(_service.AgeLimits.Distinct().Count(), _service.AgeLimits.Count);
    }

    [Fact]
    public void Channels_ReturnsAllExpectedEntries()
    {
        Assert.Equal(ExpectedChannels, _service.Channels);
    }

    [Fact]
    public void Channels_HasNoDuplicates()
    {
        Assert.Equal(_service.Channels.Distinct().Count(), _service.Channels.Count);
    }

    [Fact]
    public void Genres_ReturnsAllExpectedEntries()
    {
        Assert.Equal(ExpectedGenres, _service.Genres);
    }

    [Fact]
    public void Genres_HasNoDuplicates()
    {
        Assert.Equal(_service.Genres.Distinct().Count(), _service.Genres.Count);
    }

    [Fact]
    public void AllCollections_AreReadOnly()
    {
        Assert.IsAssignableFrom<IReadOnlyList<string>>(_service.AgeLimits);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(_service.Channels);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(_service.Genres);
    }

    [Fact]
    public void AllCollections_AreSameAsStaticReferenceData()
    {
        Assert.Same(BookingReferenceData.AgeLimits, _service.AgeLimits);
        Assert.Same(BookingReferenceData.Channels, _service.Channels);
        Assert.Same(BookingReferenceData.Genres, _service.Genres);
    }
}
