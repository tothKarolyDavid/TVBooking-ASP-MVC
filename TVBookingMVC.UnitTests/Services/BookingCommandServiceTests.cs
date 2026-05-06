using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.UnitTests.Infrastructure;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingCommandServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsBookingToDatabase()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context);
        var booking = BookingTestData.CreateValidBooking(roomNumber: 2);

        await service.CreateAsync(booking);

        var saved = await context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(saved);
        Assert.Equal(booking.Program, saved.Program);
        Assert.Equal(booking.Channel, saved.Channel);
        Assert.Equal(booking.Genre, saved.Genre);
        Assert.Equal(booking.RoomNumber, saved.RoomNumber);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllProperties()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context);
        var booking = BookingTestData.CreateValidBooking(roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, booking);

        var updatedStart = DateTime.UtcNow.AddDays(2);
        var updatedEnd = updatedStart.AddHours(2);
        var updatedBooking = new Booking
        {
            Id = booking.Id,
            Program = "Updated Program",
            Channel = "HBO",
            Genre = "Documentary",
            Start = updatedStart,
            End = updatedEnd,
            AgeLimit = "Adults Only",
            RoomNumber = 3
        };
        await BookingTestData.SeedRoomAsync(context, 3, "room3@hotel.com");

        await service.UpdateAsync(updatedBooking);

        var saved = await context.Bookings.FindAsync(booking.Id);
        Assert.NotNull(saved);
        Assert.Equal("Updated Program", saved.Program);
        Assert.Equal("HBO", saved.Channel);
        Assert.Equal("Documentary", saved.Genre);
        Assert.Equal(updatedStart, saved.Start);
        Assert.Equal(updatedEnd, saved.End);
        Assert.Equal("Adults Only", saved.AgeLimit);
        Assert.Equal(3, saved.RoomNumber);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookingNotFound_ThrowsInvalidOperationException()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        var nonExistent = BookingTestData.CreateValidBooking(id: 999, roomNumber: 2);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(nonExistent));
        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBookingFromDatabase()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context);
        var booking = BookingTestData.CreateValidBooking(roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, booking);

        await service.DeleteAsync(booking);

        var saved = await context.Bookings.FindAsync(booking.Id);
        Assert.Null(saved);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookingNotTracked_StillDeletesSuccessfully()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context);
        var booking = BookingTestData.CreateValidBooking(roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, booking);
        var bookingId = booking.Id;
        context.ChangeTracker.Clear();

        await service.DeleteAsync(booking);

        var saved = await context.Bookings.FindAsync(bookingId);
        Assert.Null(saved);
    }

    [Fact]
    public async Task DeleteBookingsByRoomAsync_RemovesAllBookingsForRoom()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedRoomAsync(context, 3, "room3@hotel.com");
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2, program: "Room 2 A"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2, program: "Room 2 B"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 3, program: "Room 3 A"));

        await service.DeleteBookingsByRoomAsync(2);

        var remaining = await context.Bookings.ToListAsync();
        Assert.Single(remaining);
        Assert.Equal(3, remaining[0].RoomNumber);
    }

    [Fact]
    public async Task DeleteBookingsByRoomAsync_WhenRoomHasNoBookings_DoesNotThrow()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await service.DeleteBookingsByRoomAsync(999);

        Assert.Empty(await context.Bookings.ToListAsync());
    }

    [Fact]
    public async Task ReassignBookingsByRoomAsync_UpdatesAllBookingsToNewRoom()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2, program: "A"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2, program: "B"));

        await service.ReassignBookingsByRoomAsync(2, 10);

        var all = await context.Bookings.ToListAsync();
        Assert.All(all, b => Assert.Equal(10, b.RoomNumber));
    }

    [Fact]
    public async Task ReassignBookingsByRoomAsync_WhenNoBookingsToMove_DoesNotThrow()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingCommandService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        await service.ReassignBookingsByRoomAsync(999, 10);

        var all = await context.Bookings.ToListAsync();
        Assert.Single(all);
        Assert.Equal(2, all[0].RoomNumber);
    }
}
