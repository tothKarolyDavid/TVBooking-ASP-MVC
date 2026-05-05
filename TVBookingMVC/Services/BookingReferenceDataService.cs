namespace TVBookingMVC.Services;

public interface IBookingReferenceDataService
{
    IReadOnlyList<string> AgeLimits { get; }
    IReadOnlyList<string> Channels { get; }
    IReadOnlyList<string> Genres { get; }
}

public sealed class BookingReferenceDataService : IBookingReferenceDataService
{
    public IReadOnlyList<string> AgeLimits { get; } = [
        "Child-friendly Program",
        "General Audience",
        "Under 6 not recommended",
        "Under 12 not recommended",
        "Under 16 not recommended",
        "Under 18 not recommended",
        "Adults Only",
    ];

    public IReadOnlyList<string> Channels { get; } = [
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
        "Eurosport 2",
    ];

    public IReadOnlyList<string> Genres { get; } = [
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
}