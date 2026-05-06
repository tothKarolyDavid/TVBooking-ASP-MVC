namespace TVBookingMVC.ViewModels;

public class StatisticsViewModel
{
    public List<ChannelViewer> ChannelViewers { get; set; } = [];
    public List<GenreViewer> GenreViewers { get; set; } = [];
    public List<DateViewer> DateViewers { get; set; } = [];

    public int TotalBookings { get; set; }
    public int TotalMinutes { get; set; }
    public int ActiveChannels { get; set; }
    public int GenreCount { get; set; }

    public string? MostPopularChannel { get; set; }
    public int MostPopularChannelBookings { get; set; }
    public string? MostPopularGenre { get; set; }
    public int MostPopularGenreBookings { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class DateViewer
{
    public DateTime Date { get; set; }
    public int Minutes { get; set; }
}

public class GenreViewer
{
    public string? Genre { get; set; }
    public int Viewers { get; set; }
}

public class ChannelViewer
{
    public string? Channel { get; set; }
    public int Viewers { get; set; }
}
