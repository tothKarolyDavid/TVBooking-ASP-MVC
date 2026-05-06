using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Models;
using TVBookingMVC.ViewModels;

namespace TVBookingMVC.Services;

public interface IBookingQueryService
{
    Task<List<Booking>> GetAllBookingsAsync();
    Task<List<Booking>> GetNearBookingsAsync();
    Task<List<Booking>> GetBookingsByAgeLimitsAsync(IEnumerable<string> ageLimits);
    Task<List<Booking>> GetFilteredBookingsAsync(string[]? ageLimits = null, int? roomNumber = null);
    Task<List<FreeTimeSlot>> GetFreeTimeSlotsAsync();
    Task<StatisticsViewModel> GetStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<List<Booking>> GetBookingsForDateAsync(DateTime date);
}

public sealed class BookingQueryService : IBookingQueryService
{
    private readonly ApplicationDbContext _context;

    public BookingQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<Booking>> GetAllBookingsAsync()
    {
        return _context.Bookings.ToListAsync();
    }

    public Task<List<Booking>> GetNearBookingsAsync()
    {
        var now = DateTime.Now;
        return _context.Bookings
            .Where(b => b.Start > now && b.Start < now.AddDays(1))
            .OrderBy(b => b.Start)
            .ToListAsync();
    }

    public Task<List<Booking>> GetBookingsByAgeLimitsAsync(IEnumerable<string> ageLimits)
    {
        var selectedAgeLimits = ageLimits.ToArray();
        return _context.Bookings
            .Where(b => selectedAgeLimits.Contains(b.AgeLimit))
            .ToListAsync();
    }

    public Task<List<Booking>> GetFilteredBookingsAsync(string[]? ageLimits = null, int? roomNumber = null)
    {
        var query = _context.Bookings.AsQueryable();

        if (ageLimits is { Length: > 0 })
        {
            query = query.Where(b => ageLimits.Contains(b.AgeLimit));
        }

        if (roomNumber.HasValue)
        {
            query = query.Where(b => b.RoomNumber == roomNumber.Value);
        }

        return query.ToListAsync();
    }

    public Task<List<FreeTimeSlot>> GetFreeTimeSlotsAsync()
    {
        var now = DateTime.Now;
        var end = now.AddDays(7);

        var bookingsReserved = _context.Bookings
            .Where(b => b.Start < end && b.End > now)
            .OrderBy(b => b.Start)
            .ToList();

        var freeTimeSlots = new List<FreeTimeSlot>();
        var start = now;

        foreach (var booking in bookingsReserved)
        {
            if (booking.Start > start)
            {
                freeTimeSlots.Add(new FreeTimeSlot { Start = start, End = booking.Start });
            }

            if (booking.End > start)
            {
                start = booking.End;
            }
        }

        if (start < end)
        {
            freeTimeSlots.Add(new FreeTimeSlot { Start = start, End = end });
        }

        return Task.FromResult(freeTimeSlots);
    }

    public async Task<StatisticsViewModel> GetStatisticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var from = (dateFrom ?? DateTime.Now.AddDays(-30)).Date;
        var to = (dateTo ?? DateTime.Now).Date;

        var bookingsQuery = _context.Bookings.Where(b => b.Start.Date >= from && b.Start.Date <= to);

        var channelViewers = await bookingsQuery
            .GroupBy(b => b.Channel)
            .Select(group => new ChannelViewer { Channel = group.Key, Viewers = group.Count() })
            .ToListAsync();

        var genreViewers = await bookingsQuery
            .GroupBy(b => b.Genre)
            .Select(group => new GenreViewer { Genre = group.Key, Viewers = group.Count() })
            .ToListAsync();

        var filteredBookings = await bookingsQuery.ToListAsync();

        var rawDateViewers = filteredBookings
            .GroupBy(b => b.Start.Date)
            .Select(g => new DateViewer { Date = g.Key, Minutes = (int)g.Sum(b => (b.End - b.Start).TotalMinutes) })
            .ToList();

        var dateViewers = new List<DateViewer>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var existing = rawDateViewers.FirstOrDefault(d => d.Date == date);
            dateViewers.Add(new DateViewer
            {
                Date = date,
                Minutes = existing?.Minutes ?? 0
            });
        }

        var orderedChannelViewers = channelViewers.OrderByDescending(cv => cv.Viewers).ToList();
        var orderedGenreViewers = genreViewers.OrderByDescending(gv => gv.Viewers).ToList();

        return new StatisticsViewModel
        {
            ChannelViewers = orderedChannelViewers,
            GenreViewers = orderedGenreViewers,
            DateViewers = dateViewers,
            TotalBookings = channelViewers.Sum(cv => cv.Viewers),
            TotalMinutes = dateViewers.Sum(dv => dv.Minutes),
            ActiveChannels = channelViewers.Count,
            GenreCount = genreViewers.Count,
            MostPopularChannel = orderedChannelViewers.FirstOrDefault()?.Channel,
            MostPopularChannelBookings = orderedChannelViewers.FirstOrDefault()?.Viewers ?? 0,
            MostPopularGenre = orderedGenreViewers.FirstOrDefault()?.Genre,
            MostPopularGenreBookings = orderedGenreViewers.FirstOrDefault()?.Viewers ?? 0,
            DateFrom = from,
            DateTo = to
        };
    }

    public Task<List<Booking>> GetBookingsForDateAsync(DateTime date)
    {
        return _context.Bookings
            .Where(b => b.Start.Date == date.Date || b.End.Date == date.Date)
            .ToListAsync();
    }
}
