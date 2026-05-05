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
    Task<List<Booking>> GetUserBookingsAsync(int roomNumber);
    Task<List<FreeTimeSlot>> GetFreeTimeSlotsAsync();
    Task<StatisticsViewModel> GetStatisticsAsync();
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

    public Task<List<Booking>> GetUserBookingsAsync(int roomNumber)
    {
        return _context.Bookings.Where(b => b.RoomNumber == roomNumber).ToListAsync();
    }

    public Task<List<FreeTimeSlot>> GetFreeTimeSlotsAsync()
    {
        var now = DateTime.Now;
        var bookingsReserved = _context.Bookings
            .Where(b => b.Start > now && b.Start < now.AddDays(7))
            .OrderBy(b => b.Start)
            .ToList();

        var freeTimeSlots = new List<FreeTimeSlot>();
        var start = now;
        var end = now.AddDays(7);

        foreach (var booking in bookingsReserved)
        {
            if (booking.Start > start)
            {
                freeTimeSlots.Add(new FreeTimeSlot { Start = start, End = booking.Start });
            }

            start = booking.End;
        }

        if (start < end)
        {
            freeTimeSlots.Add(new FreeTimeSlot { Start = start, End = end });
        }

        return Task.FromResult(freeTimeSlots);
    }

    public Task<StatisticsViewModel> GetStatisticsAsync()
    {
        var bookings = _context.Bookings.ToList();

        var channelViewers = bookings
            .GroupBy(b => b.Channel)
            .Select(group => new ChannelViewer { Channel = group.Key, Viewers = group.Count() })
            .ToList();

        var genreViewers = bookings
            .GroupBy(b => b.Genre)
            .Select(group => new GenreViewer { Genre = group.Key, Viewers = group.Count() })
            .ToList();

        var dateViewers = new List<DateViewer>();
        for (int i = 30; i >= 0; i--)
        {
            var date = DateTime.Now.AddDays(-i).Date;
            var minutes = bookings.Where(b => b.Start.Date == date).Sum(b => (b.End - b.Start).TotalMinutes);
            dateViewers.Add(new DateViewer { Date = date, Minutes = (int)minutes });
        }

        return Task.FromResult(new StatisticsViewModel
        {
            ChannelViewers = channelViewers,
            GenreViewers = genreViewers,
            DateViewers = dateViewers
        });
    }

    public Task<List<Booking>> GetBookingsForDateAsync(DateTime date)
    {
        return _context.Bookings
            .Where(b => b.Start.Date == date.Date || b.End.Date == date.Date)
            .ToListAsync();
    }
}