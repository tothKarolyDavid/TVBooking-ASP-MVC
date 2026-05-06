using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Services;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingDataSeederTests
{
    [Fact]
    public async Task SeedBookings_AddsDataToDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var count = await context.Bookings.CountAsync();

        Assert.True(count > 0, "Seed data should populate the Bookings table");
    }

    [Fact]
    public async Task SeedBookings_SeedsMultipleRooms()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var roomNumbers = await context.Bookings.Select(b => b.RoomNumber).Distinct().ToListAsync();

        Assert.True(roomNumbers.Count >= 20, "Seed data should cover rooms 2 through 21");
    }

    [Fact]
    public async Task SeedBookings_AllBookingsHaveRequiredFields()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var bookings = await context.Bookings.ToListAsync();

        Assert.All(bookings, b =>
        {
            Assert.False(string.IsNullOrWhiteSpace(b.Program));
            Assert.False(string.IsNullOrWhiteSpace(b.Channel));
            Assert.False(string.IsNullOrWhiteSpace(b.Genre));
            Assert.NotEqual(default, b.Start);
            Assert.NotEqual(default, b.End);
            Assert.False(string.IsNullOrWhiteSpace(b.AgeLimit));
            Assert.InRange(b.RoomNumber, 2, 21);
        });
    }
}
