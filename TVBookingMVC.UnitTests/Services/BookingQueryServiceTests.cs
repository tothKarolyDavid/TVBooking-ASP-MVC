using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.Constants;
using TVBookingMVC.UnitTests.Infrastructure;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingQueryServiceTests
{
    [Fact]
    public async Task GetAllBookingsAsync_WhenDbHasData_ReturnsAllBookings()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        var result = await service.GetAllBookingsAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllBookingsAsync_WhenDbIsEmpty_ReturnsEmptyList()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var result = await service.GetAllBookingsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNearBookingsAsync_ReturnsOnlyFutureBookingsWithin24HourWindow()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        var nearFuture = DateTime.Now.AddHours(2);
        var tooFar = DateTime.Now.AddHours(BookingConstants.NearBookingWindowHours + 1);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: nearFuture, roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: tooFar, roomNumber: 2, program: "Too Far"));

        var result = await service.GetNearBookingsAsync();

        Assert.Single(result);
        Assert.DoesNotContain(result, b => b.Program == "Too Far");
    }

    [Fact]
    public async Task GetNearBookingsAsync_ExcludesPastBookings()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        var past = DateTime.Now.AddHours(-2);
        var future = DateTime.Now.AddHours(2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: past, roomNumber: 2, program: "Past"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: future, roomNumber: 2, program: "Future"));

        var result = await service.GetNearBookingsAsync();

        Assert.DoesNotContain(result, b => b.Program == "Past");
        Assert.Contains(result, b => b.Program == "Future");
    }

    [Fact]
    public async Task GetNearBookingsAsync_ReturnsBookingsOrderedByStart()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: DateTime.Now.AddHours(5), roomNumber: 2, program: "Later"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(start: DateTime.Now.AddHours(2), roomNumber: 2, program: "Earlier"));

        var result = await service.GetNearBookingsAsync();

        Assert.Collection(result,
            b => Assert.Equal("Earlier", b.Program),
            b => Assert.Equal("Later", b.Program));
    }

    [Fact]
    public async Task GetNearBookingsAsync_WhenNoNearBookings_ReturnsEmpty()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var result = await service.GetNearBookingsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookingsByAgeLimitsAsync_WithSingleAgeLimit_ReturnsMatchingBookings()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Adults Only", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "General Audience", roomNumber: 2, program: "General"));

        var result = await service.GetBookingsByAgeLimitsAsync(new[] { "Adults Only" });

        Assert.Single(result);
        Assert.All(result, b => Assert.Equal("Adults Only", b.AgeLimit));
    }

    [Fact]
    public async Task GetBookingsByAgeLimitsAsync_WithMultipleAgeLimits_ReturnsAllMatching()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Adults Only", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "General Audience", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Child-friendly Program", roomNumber: 2));

        var result = await service.GetBookingsByAgeLimitsAsync(new[] { "Adults Only", "General Audience" });

        Assert.Equal(2, result.Count);
        Assert.All(result, b => Assert.Contains(b.AgeLimit, new[] { "Adults Only", "General Audience" }));
    }

    [Fact]
    public async Task GetBookingsByAgeLimitsAsync_WithNoMatches_ReturnsEmpty()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        var result = await service.GetBookingsByAgeLimitsAsync(new[] { "NonExistentLimit" });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookingsByAgeLimitsAsync_WithEmptyArray_ReturnsEmpty()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        var result = await service.GetBookingsByAgeLimitsAsync(Array.Empty<string>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_FiltersByRoomNumber()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedRoomAsync(context, 3, "room3@hotel.com");
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 3, program: "Room 3 Show"));

        var result = await service.GetFilteredBookingsAsync(roomNumber: 2);

        Assert.Single(result);
        Assert.All(result, b => Assert.Equal(2, b.RoomNumber));
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_FiltersByAgeLimitsOnly()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Adults Only", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "General Audience", roomNumber: 2, program: "General"));

        var result = await service.GetFilteredBookingsAsync(ageLimits: new[] { "Adults Only" });

        Assert.Single(result);
        Assert.All(result, b => Assert.Equal("Adults Only", b.AgeLimit));
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_FiltersByAgeLimitsAndRoomNumber()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedRoomAsync(context, 3, "room3@hotel.com");
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Adults Only", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "Adults Only", roomNumber: 3, program: "Wrong Room"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(ageLimit: "General Audience", roomNumber: 2, program: "Wrong Age"));

        var result = await service.GetFilteredBookingsAsync(ageLimits: new[] { "Adults Only" }, roomNumber: 2);

        Assert.Single(result);
        Assert.All(result, b => { Assert.Equal(2, b.RoomNumber); Assert.Equal("Adults Only", b.AgeLimit); });
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_WithNoFilters_ReturnsAllBookings()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedRoomAsync(context, 3, "room3@hotel.com");
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 3));

        var result = await service.GetFilteredBookingsAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_WithNonExistentRoom_ReturnsEmpty()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        var result = await service.GetFilteredBookingsAsync(roomNumber: 999);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFilteredBookingsAsync_WithEmptyAgeLimits_ReturnsAll()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(roomNumber: 2));

        var result = await service.GetFilteredBookingsAsync(ageLimits: []);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookingExists_ReturnsBooking()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context);
        var booking = BookingTestData.CreateValidBooking(roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, booking);

        var result = await service.GetByIdAsync(booking.Id);

        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.Program, result.Program);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookingDoesNotExist_ReturnsNull()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFreeTimeSlotsAsync_WhenNoBookings_ReturnsSingleSlotFromNowToEnd()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var result = await service.GetFreeTimeSlotsAsync();

        Assert.Single(result);
        Assert.True(result[0].Start <= DateTime.Now);
        Assert.True(result[0].End > DateTime.Now);
    }

    [Fact]
    public async Task GetFreeTimeSlotsAsync_ReturnsGapsBetweenBookings()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedOverlappingScenarioAsync(context);

        var result = await service.GetFreeTimeSlotsAsync();

        Assert.NotEmpty(result);
        foreach (var slot in result)
        {
            Assert.True(slot.End > slot.Start);
        }
    }

    [Fact]
    public async Task GetFreeTimeSlotsAsync_EmptyWhenBookingsCoverFullRange()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var now = DateTime.Now;
        await BookingTestData.SeedRoomAsync(context, 2);
        var longBooking = BookingTestData.CreateValidBooking(
            start: now.AddMinutes(-5),
            end: now.AddDays(BookingConstants.FreeSlotLookAheadDays).AddMinutes(5),
            roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, longBooking);

        var result = await service.GetFreeTimeSlotsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBookingsForDateAsync_ReturnsBookingsOnGivenDate()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        var targetDate = DateTime.UtcNow.AddDays(5).Date;
        var onDate = BookingTestData.CreateValidBooking(
            start: targetDate.AddHours(10), end: targetDate.AddHours(11), roomNumber: 2, program: "On Date");
        var otherDate = BookingTestData.CreateValidBooking(
            start: targetDate.AddDays(1).AddHours(10), roomNumber: 2, program: "Other Date");
        await BookingTestData.SeedBookingAsync(context, onDate);
        await BookingTestData.SeedBookingAsync(context, otherDate);

        var result = await service.GetBookingsForDateAsync(targetDate);

        Assert.Single(result);
        Assert.Equal("On Date", result[0].Program);
    }

    [Fact]
    public async Task GetBookingsForDateAsync_ReturnsBookingSpanningMidnight()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        var targetDate = DateTime.UtcNow.AddDays(5).Date;
        var spansMidnight = BookingTestData.CreateValidBooking(
            start: targetDate.AddHours(23), end: targetDate.AddDays(1).AddHours(1),
            roomNumber: 2, program: "Spans Midnight");
        await BookingTestData.SeedBookingAsync(context, spansMidnight);

        var resultOnStart = await service.GetBookingsForDateAsync(targetDate);
        var resultOnEnd = await service.GetBookingsForDateAsync(targetDate.AddDays(1));

        Assert.Single(resultOnStart);
        Assert.Single(resultOnEnd);
    }

    [Fact]
    public async Task GetBookingsForDateAsync_WithNoBookings_ReturnsEmpty()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var result = await service.GetBookingsForDateAsync(DateTime.UtcNow.AddDays(10));

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsModelWithPopulatedCollections()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedOverlappingScenarioAsync(context);

        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow.AddDays(2);
        var result = await service.GetStatisticsAsync(dateFrom: from, dateTo: to);

        Assert.NotNull(result.ChannelViewers);
        Assert.NotNull(result.GenreViewers);
        Assert.NotNull(result.DateViewers);
        Assert.True(result.TotalBookings > 0);
        Assert.True(result.TotalMinutes > 0);
        Assert.True(result.ActiveChannels > 0);
        Assert.True(result.GenreCount > 0);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithCustomDateRange_FiltersCorrectly()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        var dateInRange = DateTime.UtcNow.AddDays(-5).Date;
        var dateOutOfRange = DateTime.UtcNow.AddDays(-60).Date;
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(
            start: dateInRange.AddHours(10), roomNumber: 2, program: "In Range"));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(
            start: dateOutOfRange.AddHours(10), roomNumber: 2, program: "Out of Range"));

        var result = await service.GetStatisticsAsync(dateFrom: DateTime.UtcNow.AddDays(-10), dateTo: DateTime.UtcNow);

        Assert.Contains(result.ChannelViewers, c => c.Viewers >= 1);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithRangeHavingNoBookings_ReturnsZeroValues()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var futureFrom = DateTime.UtcNow.AddDays(100);
        var futureTo = DateTime.UtcNow.AddDays(130);

        var result = await service.GetStatisticsAsync(dateFrom: futureFrom, dateTo: futureTo);

        Assert.Equal(0, result.TotalBookings);
        Assert.Equal(0, result.TotalMinutes);
        Assert.Equal(0, result.ActiveChannels);
        Assert.Equal(0, result.GenreCount);
        Assert.Null(result.MostPopularChannel);
        Assert.Null(result.MostPopularGenre);
    }

    [Fact]
    public async Task GetStatisticsAsync_DateViewersCoversFullRangeWithZeros()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc);

        var result = await service.GetStatisticsAsync(dateFrom: from, dateTo: to);

        Assert.Equal(5, result.DateViewers.Count);
        Assert.All(result.DateViewers, dv => Assert.Equal(0, dv.Minutes));
    }

    [Fact]
    public async Task GetStatisticsAsync_MostPopularChannelAndGenre_ComputedCorrectly()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingQueryService(context);

        await BookingTestData.SeedRoomAsync(context, 2);
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(channel: "HBO", genre: "Action", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(channel: "HBO", genre: "Action", roomNumber: 2));
        await BookingTestData.SeedBookingAsync(context, BookingTestData.CreateValidBooking(channel: "M1", genre: "Comedy", roomNumber: 2));

        var result = await service.GetStatisticsAsync(
            dateFrom: DateTime.UtcNow.AddDays(-1), dateTo: DateTime.UtcNow.AddDays(10));

        Assert.Equal("HBO", result.MostPopularChannel);
        Assert.Equal("Action", result.MostPopularGenre);
        Assert.True(result.MostPopularChannelBookings >= 2);
        Assert.True(result.MostPopularGenreBookings >= 2);
    }
}
