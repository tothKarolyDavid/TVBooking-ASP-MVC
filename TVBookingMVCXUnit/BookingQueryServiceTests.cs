using Microsoft.EntityFrameworkCore;
using Moq;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Services;

namespace TVBookingMVCXUnit;

public class BookingQueryServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetAllBookingsAsync_ReturnsAllBookings()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetAllBookingsAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetNearBookingsAsync_ReturnsOnlyFutureNextDayBookings()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetNearBookingsAsync();

        Assert.NotNull(result);
        foreach (var booking in result)
        {
            Assert.True(booking.Start > DateTime.Now);
            Assert.True(booking.Start < DateTime.Now.AddDays(1));
        }
    }

    [Fact]
    public async Task GetBookingsByAgeLimitsAsync_ReturnsMatchingBookings()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetBookingsByAgeLimitsAsync(new[] { "Gyermekbarát program" });

        Assert.NotNull(result);
        foreach (var booking in result)
        {
            Assert.Equal("Gyermekbarát program", booking.AgeLimit);
        }
    }

    [Fact]
    public async Task GetUserBookingsAsync_ReturnsOnlyBookingsForRoomNumber()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetUserBookingsAsync(2);

        Assert.NotNull(result);
        foreach (var booking in result)
        {
            Assert.Equal(2, booking.RoomNumber);
        }
    }

    [Fact]
    public async Task GetFreeTimeSlotsAsync_ReturnsGapsBetweenBookings()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetFreeTimeSlotsAsync();

        Assert.NotNull(result);
        foreach (var slot in result)
        {
            Assert.True(slot.End > slot.Start, "Free slot end should be after start");
        }
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsModelWithPopulatedCollections()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var result = await service.GetStatisticsAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.ChannelViewers);
        Assert.NotNull(result.GenreViewers);
        Assert.NotNull(result.DateViewers);
        Assert.Equal(31, result.DateViewers.Count); // 30 days + today
    }

    [Fact]
    public async Task GetBookingsForDateAsync_ReturnsBookingsOnGivenDate()
    {
        var context = CreateInMemoryContext();
        var service = new BookingQueryService(context);

        var today = DateTime.Now.Date;
        var result = await service.GetBookingsForDateAsync(today);

        Assert.NotNull(result);
        foreach (var booking in result)
        {
            Assert.True(
                booking.Start.Date == today || booking.End.Date == today,
                $"Booking {booking.Id} should have start or end on {today}");
        }
    }
}