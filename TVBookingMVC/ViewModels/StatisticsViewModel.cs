namespace TVBookingMVC.ViewModels;

public class StatisticsViewModel
{
    public string? ChannelViewersJson { get; set; }
    public string? GenreViewersJson { get; set; }
    public string? DateViewersJson { get; set; }
}

public class DateViewer
{
    public DateTime Date { get; set; }
    public int Viewers { get; set; }
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
