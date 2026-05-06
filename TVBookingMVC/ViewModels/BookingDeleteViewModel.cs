using TVBookingMVC.Models;

namespace TVBookingMVC.ViewModels;

public class BookingDeleteViewModel
{
    public Booking Booking { get; set; } = new();
    public string[] ReturnAgeLimits { get; set; } = [];
    public string? ReturnUrl { get; set; }
}
