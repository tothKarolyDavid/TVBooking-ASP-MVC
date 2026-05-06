using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Models;

namespace TVBookingMVC.Services;

public record BookingValidationError(string Field, string Message);

public interface IBookingValidationService
{
    Task<IReadOnlyList<BookingValidationError>> ValidateAsync(Booking booking, int? excludeBookingId = null, DateTime? now = null);
}

public class BookingValidationService : IBookingValidationService
{
    private readonly ApplicationDbContext _context;

    public BookingValidationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BookingValidationError>> ValidateAsync(Booking booking, int? excludeBookingId = null, DateTime? now = null)
    {
        var errors = new List<BookingValidationError>();
        var referenceNow = now ?? DateTime.Now;

        var roomExists = await _context.Users.AnyAsync(u => u.RoomNumber == booking.RoomNumber);
        if (!roomExists)
        {
            errors.Add(new BookingValidationError(nameof(Booking.RoomNumber), "There is no guest registered in this room"));
        }

        if (booking.End <= booking.Start)
        {
            errors.Add(new BookingValidationError(nameof(Booking.End), "The end time must be after the start time"));
        }

        if (booking.Start < referenceNow)
        {
            errors.Add(new BookingValidationError(nameof(Booking.Start), "The booking start time must be in the future"));
        }

        var overlap = await _context.Bookings.AnyAsync(old =>
            (excludeBookingId == null || old.Id != excludeBookingId) &&
            old.Start < booking.End &&
            old.End > booking.Start);

        if (overlap)
        {
            errors.Add(new BookingValidationError(nameof(Booking.Start), "The booking interval is already booked by someone"));
        }

        return errors;
    }
}
