using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Models;

namespace TVBookingMVC.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class GuestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GuestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.RoomNumber).ToListAsync();
        var assignedRooms = users.Select(u => u.RoomNumber).ToHashSet();
        ViewBag.AvailableRooms = Enumerable.Range(1, 999).Where(r => !assignedRooms.Contains(r)).ToList();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (await _userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            TempData["ErrorMessage"] = "Cannot delete the admin user.";
            return RedirectToAction(nameof(Index));
        }

        _context.Bookings.RemoveRange(
            await _context.Bookings.Where(b => b.RoomNumber == user.RoomNumber).ToListAsync());
        await _context.SaveChangesAsync();

        var email = user.Email;
        await _userManager.DeleteAsync(user);

        TempData["Message"] = $"Guest {email} deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoom(string id, int roomNumber)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.RoomNumber == roomNumber && u.Id != id);
        if (existingUser != null)
        {
            TempData["ErrorMessage"] = $"Room {roomNumber} is already occupied.";
            return RedirectToAction(nameof(Index));
        }

        user.RoomNumber = roomNumber;
        await _userManager.UpdateAsync(user);

        TempData["Message"] = $"{user.Email} assigned to room {roomNumber}.";
        return RedirectToAction(nameof(Index));
    }
}
