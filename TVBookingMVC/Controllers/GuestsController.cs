using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.ViewModels;

namespace TVBookingMVC.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class GuestsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingCommandService _bookingCommandService;
    private readonly ILogger<GuestsController> _logger;

    public GuestsController(
        UserManager<ApplicationUser> userManager,
        IBookingCommandService bookingCommandService,
        ILogger<GuestsController> logger)
    {
        _userManager = userManager;
        _bookingCommandService = bookingCommandService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.OrderBy(u => u.RoomNumber).ToListAsync();
        var assignedRooms = users.Select(u => u.RoomNumber).ToHashSet();
        var availableRooms = Enumerable.Range(BookingConstants.MinRoomNumber, BookingConstants.MaxRoomNumber)
            .Where(r => !assignedRooms.Contains(r)).ToList();

        var vm = new ManageGuestsViewModel
        {
            Users = users,
            AvailableRooms = availableRooms
        };

        return View(vm);
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

        await _bookingCommandService.DeleteBookingsByRoomAsync(user.RoomNumber);

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

        var oldRoomNumber = user.RoomNumber;
        user.RoomNumber = roomNumber;

        try
        {
            await _userManager.UpdateAsync(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE constraint") == true)
        {
            _logger.LogWarning(ex, "Room {Room} assigned to {User} by another admin concurrently", roomNumber, id);
            TempData["ErrorMessage"] = $"Room {roomNumber} was just assigned to another guest.";
            return RedirectToAction(nameof(Index));
        }

        await _bookingCommandService.ReassignBookingsByRoomAsync(oldRoomNumber, roomNumber);

        TempData["Message"] = $"{user.Email} assigned to room {roomNumber}.";
        return RedirectToAction(nameof(Index));
    }
}
