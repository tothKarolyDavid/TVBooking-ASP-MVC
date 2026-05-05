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
    public async Task<IActionResult> Index()
    {
        ViewBag.NearBookings = await _bookingQueryService.GetNearBookingsAsync();
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.SelectedAgeLimits = Array.Empty<string>();

        return View(await _bookingQueryService.GetAllBookingsAsync());
    }

    // GET: Booking/Details/5
    public async Task<IActionResult> Details(int? id)
    {
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
    public IActionResult Create()
    {
        ViewBag.Channels = _bookingReferenceDataService.Channels;
        ViewBag.Genres = _bookingReferenceDataService.Genres;
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;

        if (User.IsInRole(RoleNames.Admin))
        {
            return View();
        }
        else
        {
            var roomNumber = _context.Users.Where(u => User.Identity != null && u.UserName == User.Identity.Name).Select(u => u.RoomNumber).FirstOrDefault();
            return View(new Booking { RoomNumber = roomNumber });
        }
    }

    // POST: Booking/Create
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Program,Channel,Genre,Start,End,AgeLimit,RoomNumber")] Booking booking)
    {
        var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
        if (currentRoomNumber == null)
        {
            return Forbid();
        }

        if (!User.IsInRole(RoleNames.Admin))
        {
            booking.RoomNumber = currentRoomNumber.Value;
        }

        if (ModelState.IsValid)
        {
            var user = _context.Users.FirstOrDefault(u => u.RoomNumber == booking.RoomNumber);
            if (user == null)
            {
                ModelState.AddModelError(nameof(Booking.RoomNumber), "There is no guest registered in this room");
                return View(booking);
            }

            var validationErrors = await _bookingValidationService.ValidateAsync(booking);
            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(error.Field, error.Message);
            }

            if (!ModelState.IsValid)
            {
                return View(booking);
            }

            _context.Add(booking);

            TempData["Message"] = "Booking created successfully";

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // GET: Booking/Edit/5
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }
        return View(booking);
    }

    // POST: Booking/Edit/5
    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Program,Channel,Genre,Start,End,AgeLimit,RoomNumber")] Booking booking)
    {
        if (id != booking.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var validationErrors = await _bookingValidationService.ValidateAsync(booking, booking.Id);
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(error.Field, error.Message);
                }

                if (!ModelState.IsValid)
                {
                    return View(booking);
                }

                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // GET: Booking/Delete/5
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
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

    // POST: Booking/Delete/5
    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
        }

        TempData["Message"] = "Booking deleted successfully";
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.Id == id);
    }

    [Authorize]
    public async Task<IActionResult> UserBookings()
    {
        var currentRoomNumber = await GetCurrentUserRoomNumberAsync();
        if (currentRoomNumber == null)
        {
            return Forbid();
        }

        var bookings = await _bookingQueryService.GetUserBookingsAsync(currentRoomNumber.Value);
        return View(bookings);
    }

    public async Task<IActionResult> FreeTimeSlots()
    {
        return View(await _bookingQueryService.GetFreeTimeSlotsAsync());
    }

    [HttpGet, ActionName("IndexWithFilter")]
    public async Task<IActionResult> IndexWithFilter()
    {
        var ageLimits = Request.Query["ageLimit"].ToArray() ?? [];
        ViewBag.AgeLimits = _bookingReferenceDataService.AgeLimits;
        ViewBag.SelectedAgeLimits = ageLimits;
        ViewBag.NearBookings = await _bookingQueryService.GetNearBookingsAsync();

        if (ageLimits.Length == 0)
        {
            return View("Index", await _bookingQueryService.GetAllBookingsAsync());
        }

        return View("Index", await _bookingQueryService.GetBookingsByAgeLimitsAsync(ageLimits.Select(a => a ?? string.Empty)));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Statistics()
    {
        return View(await _bookingQueryService.GetStatisticsAsync());
    }

    [HttpGet, ActionName("XmlExport")]
    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult XmlExport()
    {
        return View();
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
            start.InnerText = b.Start.ToString(CultureInfo.InvariantCulture);
            booking.AppendChild(start);

            XmlElement end = doc.CreateElement("end");
            end.InnerText = b.End.ToString(CultureInfo.InvariantCulture);
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
        using var stream = new MemoryStream();
        doc.Save(stream);
        stream.Position = 0;

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
