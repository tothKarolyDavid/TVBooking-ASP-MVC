using TVBookingMVC.Models;

namespace TVBookingMVC.Services;

public interface IBookingReferenceDataService
{
    IReadOnlyList<string> AgeLimits { get; }
    IReadOnlyList<string> Channels { get; }
    IReadOnlyList<string> Genres { get; }
}

public sealed class BookingReferenceDataService : IBookingReferenceDataService
{
    public IReadOnlyList<string> AgeLimits => BookingReferenceData.AgeLimits;
    public IReadOnlyList<string> Channels => BookingReferenceData.Channels;
    public IReadOnlyList<string> Genres => BookingReferenceData.Genres;
}
