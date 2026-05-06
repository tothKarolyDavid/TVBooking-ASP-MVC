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

public class BookingController : Controller
{
    private readonly IBookingQueryService _bookingQueryService;
    private readonly IBookingCommandService _bookingCommandService;
    private readonly IBookingReferenceDataService _bookingReferenceDataService;
    private readonly IBookingValidationService _bookingValidationService;
    private readonly IBookingExportService _bookingExportService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BookingController> _logger;

    public BookingController(
        IBookingQueryService bookingQueryService,
        IBookingCommandService bookingCommandService,
        IBookingReferenceDataService bookingReferenceDataService,
        IBookingValidationService bookingValidationService,
        IBookingExportService bookingExportService,
        UserManager<ApplicationUser> userManager,
        ILogger<BookingController> logger)
    {
        _bookingQueryService = bookingQueryService;
        _bookingCommandService = bookingCommandService;
        _bookingReferenceDataService = bookingReferenceDataService;
        _bookingValidationService = bookingValidationService;
        _bookingExportService = bookingExportService;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Booking
    public async Task<IActionResult> Index(string[]? ageLimit = null, bool myBookings = false)
    {
        int? roomNumber = null;
        int? currentUserRoomNumber = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            currentUserRoomNumber = user?.RoomNumber;
            if (myBookings)
            {
                roomNumber = currentUserRoomNumber;
            }
        }

        var vm = new BookingIndexViewModel
        {
            Bookings = await _bookingQueryService.GetFilteredBookingsAsync(ageLimit, roomNumber),
            AgeLimits = _bookingReferenceDataService.AgeLimits,
            SelectedAgeLimits = ageLimit ?? [],
            NearBookings = await _bookingQueryService.GetNearBookingsAsync(),
            MyBookings = myBookings,
            CurrentUserRoomNumber = currentUserRoomNumber
        };

        return View(vm);
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int? id, string[]? ageLimit = null)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _bookingQueryService.GetByIdAsync(id.Value);
        if (booking == null)
        {
            return NotFound();
        }

        var vm = new BookingDeleteViewModel
        {
            Booking = booking,
            ReturnAgeLimits = ageLimit ?? []
        };

        return View(vm);
    }

    // GET: Booking/Create
    [Authorize]
    public async Task<IActionResult> Create(string[]? ageLimit = null)
    {
        int? roomNumber = null;
        if (!User.IsInRole(RoleNames.Admin))
        {
            roomNumber = await GetCurrentUserRoomNumberAsync();
        }

        var now = DateTime.Now;
        var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(BookingConstants.DefaultBookingDurationHours);
        var endTime = startTime.AddHours(BookingConstants.DefaultBookingDurationHours);

        var vm = new BookingFormViewModel
        {
            Booking = new Booking
            {
                Start = startTime,
                End = endTime,
                RoomNumber = roomNumber ?? BookingConstants.AdminRoomNumber
            },
            Channels = _bookingReferenceDataService.Channels,
            Genres = _bookingReferenceDataService.Genres,
            AgeLimits = _bookingReferenceDataService.AgeLimits,
            ReturnAgeLimits = ageLimit ?? [],
            FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync()
        };

        return View(vm);
    }

    // POST: Booking/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingFormViewModel vm, string[]? ageLimit = null)
    {
        vm.ReturnAgeLimits = ageLimit ?? [];
        var booking = vm.Booking;
        var currentRoomNumber = await GetCurrentUserRoomNumberAsync();

        if (currentRoomNumber == null)
        {
            return Forbid();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            booking.RoomNumber = currentRoomNumber.Value;
        }

        vm.Channels = _bookingReferenceDataService.Channels;
        vm.Genres = _bookingReferenceDataService.Genres;
        vm.AgeLimits = _bookingReferenceDataService.AgeLimits;
        vm.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        if (ModelState.IsValid)
        {
            var validationErrors = await _bookingValidationService.ValidateAsync(booking);
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError($"Booking.{error.Field}", error.Message);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                await _bookingCommandService.CreateAsync(booking);
                TempData["Message"] = "Booking created successfully";
                return RedirectToAction(nameof(Index), new { ageLimit });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to create booking");
                TempData["ErrorMessage"] = "Failed to create booking. Please try again.";
                return View(vm);
            }
        }

        return View(vm);
    }

    // GET: Booking/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id, string[]? ageLimit = null, string? returnUrl = null)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _bookingQueryService.GetByIdAsync(id.Value);
        if (booking == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
            if (currentRoomNumber == null || booking.RoomNumber != currentRoomNumber.Value)
            {
                return Forbid();
            }
        }

        var vm = new BookingFormViewModel
        {
            Booking = booking,
            Channels = _bookingReferenceDataService.Channels,
            Genres = _bookingReferenceDataService.Genres,
            AgeLimits = _bookingReferenceDataService.AgeLimits,
            ReturnAgeLimits = ageLimit ?? [],
            ReturnUrl = returnUrl,
            FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync()
        };

        return View(vm);
    }

    // POST: Booking/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookingFormViewModel vm, string[]? ageLimit = null, string? returnUrl = null)
    {
        vm.ReturnAgeLimits = ageLimit ?? [];
        vm.ReturnUrl = returnUrl;

        if (id != vm.Booking.Id)
        {
            return NotFound();
        }

        var trackedBooking = await _bookingQueryService.GetByIdAsync(id);
        if (trackedBooking == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
            if (currentRoomNumber == null || trackedBooking.RoomNumber != currentRoomNumber.Value)
            {
                return Forbid();
            }

            vm.Booking.RoomNumber = currentRoomNumber.Value;
        }

        vm.Channels = _bookingReferenceDataService.Channels;
        vm.Genres = _bookingReferenceDataService.Genres;
        vm.AgeLimits = _bookingReferenceDataService.AgeLimits;
        vm.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        if (ModelState.IsValid)
        {
            var validationErrors = await _bookingValidationService.ValidateAsync(vm.Booking, vm.Booking.Id);
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError($"Booking.{error.Field}", error.Message);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookingCommandService.UpdateAsync(vm.Booking);
                    TempData["Message"] = "Booking updated successfully";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction(nameof(Index), new { ageLimit });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Failed to update booking {BookingId}", id);
                    TempData["ErrorMessage"] = "Failed to update booking. Please try again.";
                    return View(vm);
                }
            }
        }

        return View(vm);
    }

    // GET: Booking/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id, string[]? ageLimit = null, string? returnUrl = null)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _bookingQueryService.GetByIdAsync(id.Value);
        if (booking == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
            if (currentRoomNumber == null || booking.RoomNumber != currentRoomNumber.Value)
            {
                return Forbid();
            }
        }

        var vm = new BookingDeleteViewModel
        {
            Booking = booking,
            ReturnAgeLimits = ageLimit ?? [],
            ReturnUrl = returnUrl
        };

        return View(vm);
    }

    // POST: Booking/Delete/5
    [Authorize]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string[]? ageLimit = null, string? returnUrl = null)
    {
        var booking = await _bookingQueryService.GetByIdAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
            if (currentRoomNumber == null || booking.RoomNumber != currentRoomNumber.Value)
            {
                return Forbid();
            }
        }

        try
        {
            await _bookingCommandService.DeleteAsync(booking);
            TempData["Message"] = "Booking deleted successfully";
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete booking {BookingId}", id);
            TempData["ErrorMessage"] = "Failed to delete booking. Please try again.";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToAction(nameof(Index), new { ageLimit });
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Statistics(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        return View(await _bookingQueryService.GetStatisticsAsync(dateFrom, dateTo));
    }

    [HttpPost, ActionName("XmlExport")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> XmlExportPost(DateTime date)
    {
        var bytes = await _bookingExportService.ExportBookingsToXmlAsync(date);
        var fileName = $"bookings_{date:yyyy-MM-dd}.xml";
        return File(bytes, "application/xml", fileName);
    }

    private async Task<int?> GetCurrentUserRoomNumberAsync()
    {
        if (User.Identity == null || string.IsNullOrWhiteSpace(User.Identity.Name))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(User.Identity.Name);
        return user?.RoomNumber;
    }
}
