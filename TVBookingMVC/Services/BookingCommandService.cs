using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Models;

namespace TVBookingMVC.Services;

public interface IBookingCommandService
{
    Task CreateAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Booking booking);
    Task DeleteBookingsByRoomAsync(int roomNumber);
    Task ReassignBookingsByRoomAsync(int oldRoomNumber, int newRoomNumber);
}

public sealed class BookingCommandService : IBookingCommandService
{
    private readonly ApplicationDbContext _context;

    public BookingCommandService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Booking booking)
    {
        _context.Add(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        var tracked = await _context.Bookings.FindAsync(booking.Id);
        if (tracked == null)
        {
            throw new InvalidOperationException($"Booking with id {booking.Id} not found.");
        }

        tracked.Program = booking.Program;
        tracked.Channel = booking.Channel;
        tracked.Genre = booking.Genre;
        tracked.Start = booking.Start;
        tracked.End = booking.End;
        tracked.AgeLimit = booking.AgeLimit;
        tracked.RoomNumber = booking.RoomNumber;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Booking booking)
    {
        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookingsByRoomAsync(int roomNumber)
    {
        _context.Bookings.RemoveRange(
            await _context.Bookings.Where(b => b.RoomNumber == roomNumber).ToListAsync());
        await _context.SaveChangesAsync();
    }

    public async Task ReassignBookingsByRoomAsync(int oldRoomNumber, int newRoomNumber)
    {
        var bookings = await _context.Bookings
            .Where(b => b.RoomNumber == oldRoomNumber)
            .ToListAsync();
        foreach (var booking in bookings)
        {
            booking.RoomNumber = newRoomNumber;
        }
        await _context.SaveChangesAsync();
    }
}
