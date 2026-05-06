using TVBookingMVC.Models;

namespace TVBookingMVC.ViewModels;

public class BookingIndexViewModel
{
    public List<Booking> Bookings { get; set; } = [];
    public IEnumerable<string> AgeLimits { get; set; } = [];
    public string[] SelectedAgeLimits { get; set; } = [];
    public List<Booking> NearBookings { get; set; } = [];
    public bool MyBookings { get; set; }
    public int? CurrentUserRoomNumber { get; set; }
}
