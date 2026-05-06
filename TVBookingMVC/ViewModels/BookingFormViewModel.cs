using TVBookingMVC.Models;

namespace TVBookingMVC.ViewModels;

public class BookingFormViewModel
{
    public Booking Booking { get; set; } = new();
    public IEnumerable<string> Channels { get; set; } = [];
    public IEnumerable<string> Genres { get; set; } = [];
    public IEnumerable<string> AgeLimits { get; set; } = [];
    public string[] ReturnAgeLimits { get; set; } = [];
    public string? ReturnUrl { get; set; }
    public List<FreeTimeSlot> FreeTimeSlots { get; set; } = [];
}
