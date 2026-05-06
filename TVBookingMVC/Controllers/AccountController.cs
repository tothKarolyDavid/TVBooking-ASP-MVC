using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Services;

namespace TVBookingMVC.Controllers;

public class AccountController : Controller
{
    private readonly IBookingCommandService _bookingCommandService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        IBookingCommandService bookingCommandService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _bookingCommandService = bookingCommandService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Index", "Booking");
        }

        if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            return RedirectToAction("Index", "Booking");
        }

        await _bookingCommandService.DeleteBookingsByRoomAsync(user.RoomNumber);
        await _userManager.DeleteAsync(user);
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Booking");
    }
}
