using TVBookingMVC.Areas.Identity.Data;

namespace TVBookingMVC.ViewModels;

public class ManageGuestsViewModel
{
    public List<ApplicationUser> Users { get; set; } = [];
    public List<int> AvailableRooms { get; set; } = [];
}
