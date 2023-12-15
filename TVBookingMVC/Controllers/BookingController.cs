using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Xml;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Models;

namespace TVBookingMVC.Controllers;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Booking
    public async Task<IActionResult> Index()
    {
        TempData["NearBookings"] = _context.Bookings.Where(b => b.Start > DateTime.Now && b.Start < DateTime.Now.AddDays(1)).OrderBy(b => b.Start).ToList();

        return View(await _context.Bookings.ToListAsync());
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
    public IActionResult Create()
    {
        if (Globals.IsAdmin)
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Program,Channel,Genre,Start,End,AgeLimit,RoomNumber")] Booking booking)
    {
        if (ModelState.IsValid)
        {
            var user = _context.Users.FirstOrDefault(u => u.RoomNumber == booking.RoomNumber);
            if (user == null)
            {
                ModelState.AddModelError(nameof(Booking.RoomNumber), "There is no guest registered in this room");
                return View(booking);
            }

            if (booking.End < booking.Start)
            {
                ModelState.AddModelError(nameof(Booking.End), "The end time must be after the start time");
                return View(booking);
            }

            if (booking.Start < DateTime.Now)
            {
                ModelState.AddModelError(nameof(Booking.Start), "The booking start time must be in the future");
                return View(booking);
            }

            if (_context.Bookings.Any(old => old.Start < booking.End && old.End > booking.Start))
            {
                ModelState.AddModelError(nameof(Booking.Start), "The booking interval is already booked by someone");
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

    public IActionResult UserBookings()
    {
        var bookings = _context.Bookings.Where(b => b.RoomNumber == _context.Users.Where(u => User.Identity != null && u.UserName == User.Identity.Name).Select(u => u.RoomNumber).FirstOrDefault()).ToList();
        return View(bookings);
    }

    public IActionResult FreeTimeSlots()
    {
        var bookingsReserved = _context.Bookings.Where(b => b.Start > DateTime.Now && b.Start < DateTime.Now.AddDays(7)).OrderBy(b => b.Start).ToList();

        var freeTimeSlots = new List<FreeTimeSlot>();
        var start = DateTime.Now;
        var end = DateTime.Now.AddDays(7);

        foreach (var booking in bookingsReserved)
        {
            if (booking.Start > start)
            {
                freeTimeSlots.Add(new FreeTimeSlot { Start = start, End = booking.Start });
            }

            start = booking.End;
        }

        return View(freeTimeSlots);
    }

    [HttpGet, ActionName("IndexWithFilter")]
    public IActionResult IndexWithFilter()
    {
        var bookings = _context.Bookings.ToList();

        var ageLimits = Request.Query["ageLimit"].ToArray();

        ViewBag.AgeLimits = ageLimits;

        if (ageLimits.Length == 0)
        {
            return View("Index", bookings);
        }

        return View("Index", bookings.Where(b => ageLimits.Contains(b.AgeLimit)).ToList());
    }

    public IActionResult Statistics()
    {
        var bookings = _context.Bookings.ToList();
        var channels = _context.Bookings.Select(b => b.Channel).Distinct().ToList();
        var genres = _context.Bookings.Select(b => b.Genre).Distinct().ToList();

        var channelViewers = new List<ChannelViewer>();
        foreach (var channel in channels)
        {
            channelViewers.Add(new ChannelViewer { Channel = channel, Viewers = bookings.Count(b => b.Channel == channel) });
        }

        var genreViewers = new List<GenreViewer>();
        foreach (var genre in genres)
        {
            genreViewers.Add(new GenreViewer { Genre = genre, Viewers = bookings.Count(b => b.Genre == genre) });
        }

        var dateViewers = new List<DateViewer>();
        for (int i = 30; i >= 0; i--)
        {
            // convert to date only
            var date = DateTime.Now.AddDays(-i).Date;

            // get the length of all bookings for the given date (in minutes) by summing the difference between the start and end times
            var viewers = bookings.Where(b => b.Start.Date == date).Sum(b => (b.End - b.Start).TotalMinutes);

            dateViewers.Add(new DateViewer { Date = date, Viewers = (int)viewers });
        }


        StatisticsModel model = new()
        {
            ChannelViewersJson = JsonConvert.SerializeObject(channelViewers),
            GenreViewersJson = JsonConvert.SerializeObject(genreViewers),
            DateViewersJson = JsonConvert.SerializeObject(dateViewers)
        };

        return View(model);
    }

    [HttpGet, ActionName("XmlExport")]
    public IActionResult XmlExport()
    {
        return View();
    }

    [HttpPost, ActionName("XmlExport")]
    public IActionResult XmlExportPost(DateTime date)
    {
        XmlDocument doc = new();

        XmlElement root = doc.CreateElement("bookings");
        doc.AppendChild(root);

        var bookings = _context.Bookings.Where(b => b.Start.Date == date.Date || b.End.Date == date.Date).ToList();
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

        doc.Save($"bookings_{date:yyyy-MM-dd}.xml");

        TempData["Message"] = "Bookings exported successfully";

        return View();
    }
}

public class StatisticsModel
{
    public string? ChannelViewersJson { get; set; }
    public string? GenreViewersJson { get; set; }
    public string? DateViewersJson { get; set; }
}

public class DateViewer
{
    public DateTime Date { get; set; }
    public int Viewers { get; set; }
}

public class GenreViewer
{
    public string? Genre { get; set; }
    public int Viewers { get; set; }
}

public class ChannelViewer
{
    public string? Channel { get; set; }
    public int Viewers { get; set; }
}

public class FreeTimeSlot
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
