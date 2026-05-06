using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Models;

namespace TVBookingMVC.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
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

        _context.Bookings.RemoveRange(
            await _context.Bookings.Where(b => b.RoomNumber == user.RoomNumber).ToListAsync());

        await _context.SaveChangesAsync();
        await _userManager.DeleteAsync(user);
        await _signInManager.SignOutAsync();

        return RedirectToAction("Index", "Booking");
    }
}
