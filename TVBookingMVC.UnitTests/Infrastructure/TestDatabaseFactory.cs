using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Models;

namespace TVBookingMVC.UnitTests.Infrastructure;

public static class TestDatabaseFactory
{
    public static async Task<ApplicationDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public static async Task<ApplicationDbContext> CreateCleanContextAsync()
    {
        var context = await CreateContextAsync();
        context.Bookings.RemoveRange(context.Bookings);
        await context.SaveChangesAsync();
        return context;
    }
}

public static class BookingTestData
{
    public static Booking CreateValidBooking(
        int? id = null,
        string program = "Test Program",
        string channel = "BBC One",
        string genre = "Action",
        DateTime? start = null,
        DateTime? end = null,
        string ageLimit = "General Audience",
        int roomNumber = 2)
    {
        var s = start ?? DateTime.UtcNow.AddDays(1);
        return new Booking
        {
            Id = id ?? 0,
            Program = program,
            Channel = channel,
            Genre = genre,
            Start = s,
            End = end ?? s.AddHours(1),
            AgeLimit = ageLimit,
            RoomNumber = roomNumber
        };
    }

    public static async Task SeedRoomAsync(ApplicationDbContext context, int roomNumber = 2, string email = "room2@hotel.com")
    {
        context.Users.Add(new ApplicationUser
        {
            UserName = email,
            Email = email,
            RoomNumber = roomNumber,
            NormalizedUserName = email.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true
        });
        await context.SaveChangesAsync();
    }

    public static async Task SeedBookingAsync(ApplicationDbContext context, Booking booking)
    {
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
    }

    public static async Task SeedOverlappingScenarioAsync(ApplicationDbContext context)
    {
        var baseTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);

        await SeedRoomAsync(context, 5, "room5@hotel.com");
        await SeedRoomAsync(context, 6, "room6@hotel.com");

        var bookings = new List<Booking>
        {
            new() { Program = "Morning News", Channel = "BBC News", Genre = "News", Start = baseTime, End = baseTime.AddHours(1), AgeLimit = "General Audience", RoomNumber = 5 },
            new() { Program = "Late Morning Show", Channel = "BBC One", Genre = "Show", Start = baseTime.AddHours(1), End = baseTime.AddHours(2), AgeLimit = "General Audience", RoomNumber = 5 },
            new() { Program = "Midday Movie", Channel = "HBO", Genre = "Movie", Start = baseTime.AddHours(3), End = baseTime.AddHours(5), AgeLimit = "Under 12 not recommended", RoomNumber = 6 },
            new() { Program = "Evening Documentary", Channel = "Discovery Channel", Genre = "Documentary", Start = baseTime.AddHours(6), End = baseTime.AddHours(7), AgeLimit = "General Audience", RoomNumber = 5 }
        };

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();
    }
}
