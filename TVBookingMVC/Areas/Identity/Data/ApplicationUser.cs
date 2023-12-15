using Microsoft.AspNetCore.Identity;

namespace TVBookingMVC.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int RoomNumber { get; set; }
}

