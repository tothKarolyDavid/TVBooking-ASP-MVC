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
        // UK Public/State Broadcasters
        "BBC One",
        "BBC Two",
        "BBC Three",
        "BBC Four",
        "ITV",
        "Channel 4",
        "Channel 5",
        "More4",
        // Premium/Cable
        "Sky Atlantic",
        "Sky Cinema",
        "Sky Premiere",
        "Sky Arts",
        "Sky Action",
        "Sky Sci-Fi",
        "HBO",
        "Cinemax",
        "Showtime",
        "AMC",
        "FX",
        "BET",
        "Starz",
        "Epix",
        // Entertainment
        "Disney Channel",
        "Nickelodeon",
        "Cartoon Network",
        "TNT",
        "TBS",
        "Comedy Central",
        "MTV",
        "VH1",
        "Lifetime",
        "HGTV",
        "E!",
        "Bravo",
        // Documentary/Educational
        "National Geographic",
        "National Geographic Wild",
        "Discovery Channel",
        "Animal Planet",
        "History Channel",
        "Smithsonian",
        "BBC Earth",
        "TLC",
        "Investigation Discovery",
        "Science Channel",
        "Nat Geo Wild",
        // Sports & News
        "ESPN",
        "CNN",
        "BBC News",
        "Sky News",
        "Eurosport 1",
        "Eurosport 2",
        "BT Sport 1",
        "BT Sport 2",
        "Golf Channel",
        "Sky Sports Main Event",
        "Fox Sports"
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
