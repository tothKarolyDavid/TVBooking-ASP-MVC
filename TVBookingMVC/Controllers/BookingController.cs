using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Xml;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.ViewModels;

namespace TVBookingMVC.Controllers;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingValidationService _bookingValidationService;
    private readonly IBookingReferenceDataService _bookingReferenceDataService;
    private readonly IBookingQueryService _bookingQueryService;

    public BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IBookingValidationService bookingValidationService,
        IBookingReferenceDataService bookingReferenceDataService,
        IBookingQueryService bookingQueryService)
    {
        _context = context;
        _userManager = userManager;
        _bookingValidationService = bookingValidationService;
        _bookingReferenceDataService = bookingReferenceDataService;
        _bookingQueryService = bookingQueryService;
    }

    // GET: Booking
    public async Task<IActionResult> Index(string[]? ageLimit = null, bool myBookings = false)
    {
        ViewBag.NearBookings = await _bookingQueryService.GetNearBookingsAsync();
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.SelectedAgeLimits = ageLimit ?? [];
        ViewBag.MyBookings = myBookings;

        int? roomNumber = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.CurrentUserRoomNumber = user?.RoomNumber;
            if (myBookings)
            {
                roomNumber = user?.RoomNumber;
            }
        }

        return View(await _bookingQueryService.GetFilteredBookingsAsync(ageLimit, roomNumber));
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int? id, string[]? ageLimit = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];

        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(m => m.Id == id);
        if (booking == null)
        {
            return NotFound();
        }

        return View(booking);
    }

    // GET: Booking/Create
    [Authorize]
    public async Task<IActionResult> Create(string[]? ageLimit = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];
        ViewBag.Channels = _bookingReferenceDataService.Channels;
        ViewBag.Genres = _bookingReferenceDataService.Genres;
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        var now = DateTime.Now;
        var startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
        var endTime = startTime.AddHours(1);

        var defaultBooking = new Booking
        {
            Start = startTime,
            End = endTime
        };

        if (User.IsInRole(RoleNames.Admin))
        {
            return View(defaultBooking);
        }
        else
        {
            var roomNumber = _context.Users.Where(u => User.Identity != null && u.UserName == User.Identity.Name).Select(u => u.RoomNumber).FirstOrDefault();
            defaultBooking.RoomNumber = roomNumber;
            return View(defaultBooking);
        }
    }

    // POST: Booking/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Program,Channel,Genre,Start,End,AgeLimit,RoomNumber")] Booking booking, string[]? ageLimit = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];
        var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
        if (currentRoomNumber == null)
        {
            return Forbid();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            booking.RoomNumber = currentRoomNumber.Value;
        }

        ViewBag.Channels = _bookingReferenceDataService.Channels;
        ViewBag.Genres = _bookingReferenceDataService.Genres;
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        if (ModelState.IsValid)
        {
            var validationErrors = await _bookingValidationService.ValidateAsync(booking);
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(error.Field, error.Message);
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Booking created successfully";
                return RedirectToAction(nameof(Index), new { ageLimit });
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Failed to create booking. Please try again.";
                return View(booking);
            }
        }
        return View(booking);
    }

    // GET: Booking/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id, string[]? ageLimit = null, string? returnUrl = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];
        ViewBag.ReturnUrl = returnUrl;
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings.FindAsync(id);
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

        ViewBag.Channels = _bookingReferenceDataService.Channels;
        ViewBag.Genres = _bookingReferenceDataService.Genres;
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        return View(booking);
    }

    // POST: Booking/Edit/5
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Program,Channel,Genre,Start,End,AgeLimit,RoomNumber")] Booking booking, string[]? ageLimit = null, string? returnUrl = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];
        ViewBag.ReturnUrl = returnUrl;
        if (id != booking.Id)
        {
            return NotFound();
        }

        var trackedBooking = await _context.Bookings.FindAsync(id);
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

            booking.RoomNumber = currentRoomNumber.Value;
        }

        ViewBag.Channels = _bookingReferenceDataService.Channels;
        ViewBag.Genres = _bookingReferenceDataService.Genres;
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.FreeTimeSlots = await _bookingQueryService.GetFreeTimeSlotsAsync();

        if (ModelState.IsValid)
        {
            var validationErrors = await _bookingValidationService.ValidateAsync(booking, booking.Id);
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(error.Field, error.Message);
            }

            if (ModelState.IsValid)
            {
                trackedBooking.Program = booking.Program;
                trackedBooking.Channel = booking.Channel;
                trackedBooking.Genre = booking.Genre;
                trackedBooking.Start = booking.Start;
                trackedBooking.End = booking.End;
                trackedBooking.AgeLimit = booking.AgeLimit;
                trackedBooking.RoomNumber = booking.RoomNumber;

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Booking updated successfully";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    return RedirectToAction(nameof(Index), new { ageLimit });
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "Failed to update booking. Please try again.";
                    return View(booking);
                }
            }
        }

        return View(booking);
    }

    // GET: Booking/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id, string[]? ageLimit = null, string? returnUrl = null)
    {
        ViewBag.ReturnAgeLimits = ageLimit ?? [];
        ViewBag.ReturnUrl = returnUrl;
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(m => m.Id == id);
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

        return View(booking);
    }

    // POST: Booking/Delete/5
    [Authorize]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string[]? ageLimit = null, string? returnUrl = null)
    {
        var booking = await _context.Bookings.FindAsync(id);
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
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Booking deleted successfully";
        }
        catch (DbUpdateException)
        {
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
        XmlDocument doc = new();

        XmlElement root = doc.CreateElement("bookings");
        doc.AppendChild(root);

        var bookings = await _bookingQueryService.GetBookingsForDateAsync(date);
        bookings.ForEach(b =>
        {
            XmlElement booking = doc.CreateElement("booking");

            XmlElement program = doc.CreateElement("program");
            program.InnerText = b.Program ?? string.Empty;
            booking.AppendChild(program);

            XmlElement channel = doc.CreateElement("channel");
            channel.InnerText = b.Channel ?? string.Empty;
            booking.AppendChild(channel);

            XmlElement genre = doc.CreateElement("genre");
            genre.InnerText = b.Genre ?? string.Empty;
            booking.AppendChild(genre);

            XmlElement start = doc.CreateElement("start");
            start.InnerText = b.Start.ToString("yyyy/MM/dd HH:mm");
            booking.AppendChild(start);

            XmlElement end = doc.CreateElement("end");
            end.InnerText = b.End.ToString("yyyy/MM/dd HH:mm");
            booking.AppendChild(end);

            XmlElement ageLimit = doc.CreateElement("ageLimit");
            ageLimit.InnerText = b.AgeLimit ?? string.Empty;
            booking.AppendChild(ageLimit);

            XmlElement roomNumber = doc.CreateElement("roomNumber");
            roomNumber.InnerText = b.RoomNumber.ToString();
            booking.AppendChild(roomNumber);

            root.AppendChild(booking);
        });

        var fileName = $"bookings_{date:yyyy-MM-dd}.xml";

        var stream = new MemoryStream();
        doc.Save(stream);
        stream.Position = 0;

        var count = bookings.Count;
        return File(stream, "application/xml", fileName);
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
