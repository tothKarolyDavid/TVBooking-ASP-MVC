using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Models;

namespace TVBookingMVC.Services;

public static class BookingDataSeeder
{
    public static void SeedBookings(this ModelBuilder builder)
    {
        var rng = new Random(41205);
        var baseDate = DateTime.Today.AddHours(8);
        int bookingId = 10;

        for (int room = 2; room < 22; room++)
        {
            for (int pastDay = 0; pastDay < 30; pastDay++)
            {
                int bookings = rng.Next(0, 5);
                for (int k = 0; k < bookings; k++)
                {
                    var hoursPerSlot = Math.Max(1, 24 / bookings);
                    var start = baseDate.AddDays(-pastDay).AddHours(rng.Next(hoursPerSlot) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {rng.Next(1, 100)}",
                        Channel = BookingReferenceData.Channels[rng.Next(0, BookingReferenceData.Channels.Length)],
                        Genre = BookingReferenceData.Genres[rng.Next(0, BookingReferenceData.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(rng.Next(30, 121)),
                        AgeLimit = BookingReferenceData.AgeLimits[rng.Next(0, BookingReferenceData.AgeLimits.Length)],
                        RoomNumber = room,
                    });
                }
            }

            for (int futureDay = 0; futureDay < 7; futureDay++)
            {
                int bookings = rng.Next(0, 3);
                for (int k = 0; k < bookings; k++)
                {
                    var hoursPerSlot = Math.Max(1, 24 / bookings);
                    var start = baseDate.AddDays(futureDay).AddHours(rng.Next(hoursPerSlot) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {rng.Next(1, 100)}",
                        Channel = BookingReferenceData.Channels[rng.Next(0, BookingReferenceData.Channels.Length)],
                        Genre = BookingReferenceData.Genres[rng.Next(0, BookingReferenceData.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(rng.Next(30, 121)),
                        AgeLimit = BookingReferenceData.AgeLimits[rng.Next(0, BookingReferenceData.AgeLimits.Length)],
                        RoomNumber = room,
                    });
                }
            }
        }
    }
}
