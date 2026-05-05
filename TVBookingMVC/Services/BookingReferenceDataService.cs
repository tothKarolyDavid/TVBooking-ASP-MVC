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
        "Gyermekbarát program",
        "Korhatárra való tekintet nélkül megtekinthető",
        "6 éven aluliak számára nem ajánlott",
        "12 éven aluliak számára nem ajánlott",
        "16 éven aluliak számára nem ajánlott",
        "18 éven aluliak számára nem ajánlott",
        "Kizárólag felnőttek számára ajánlott",
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
        "Akció",
        "Animációs",
        "Családi",
        "Dokumentum",
        "Dráma",
        "Életmód",
        "Fantasy",
        "Film",
        "Gyerek",
        "Hír",
        "Horror",
        "Ismeretterjesztő",
        "Játék",
        "Kaland",
        "Képregény",
        "Komédia",
        "Koncert",
        "Krimi",
        "Mese",
        "Mesefilm",
        "Reality",
        "Romantikus",
        "Sci-fi",
        "Show",
        "Sitcom",
        "Sport",
        "Szórakoztató",
        "Talkshow",
        "Egyéb"
    ];
}