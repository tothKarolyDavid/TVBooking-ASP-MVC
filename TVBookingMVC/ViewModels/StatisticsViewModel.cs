namespace TVBookingMVC.ViewModels;

public class StatisticsViewModel
{
    public List<ChannelViewer> ChannelViewers { get; set; } = [];
    public List<GenreViewer> GenreViewers { get; set; } = [];
    public List<DateViewer> DateViewers { get; set; } = [];
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
