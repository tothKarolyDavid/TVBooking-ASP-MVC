using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.UnitTests.Infrastructure;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingValidationServiceTests
{
    [Fact]
    public async Task ValidateAsync_WithValidBooking_ReturnsNoErrors()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        await BookingTestData.SeedRoomAsync(context);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var booking = BookingTestData.CreateValidBooking(start: futureDate, end: futureDate.AddHours(1), roomNumber: 2);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Empty(errors);
    }

    [Fact]
    public async Task ValidateAsync_WhenEndEqualsStart_ReturnsError()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        await BookingTestData.SeedRoomAsync(context);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var booking = BookingTestData.CreateValidBooking(start: futureDate, end: futureDate, roomNumber: 2);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.End));
    }

    [Fact]
    public async Task ValidateAsync_WhenEndBeforeStart_ReturnsError()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        await BookingTestData.SeedRoomAsync(context);
        var futureDate = DateTime.UtcNow.AddDays(1);
        var booking = BookingTestData.CreateValidBooking(start: futureDate, end: futureDate.AddHours(-1), roomNumber: 2);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.End));
    }

    [Fact]
    public async Task ValidateAsync_WhenStartInPast_ReturnsError()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        await BookingTestData.SeedRoomAsync(context);
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var booking = BookingTestData.CreateValidBooking(start: pastDate, end: pastDate.AddHours(1), roomNumber: 2);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.Start));
    }

    [Fact]
    public async Task ValidateAsync_WhenRoomDoesNotExist_ReturnsError()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var futureDate = DateTime.UtcNow.AddDays(1);
        var booking = BookingTestData.CreateValidBooking(start: futureDate, end: futureDate.AddHours(1), roomNumber: 999);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.RoomNumber));
    }

    [Fact]
    public async Task ValidateAsync_DetectsOverlappingBooking()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var baseTime = DateTime.UtcNow.AddDays(1);
        await BookingTestData.SeedRoomAsync(context, 2);
        var existing = BookingTestData.CreateValidBooking(start: baseTime, end: baseTime.AddHours(2), roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, existing);

        var overlapping = BookingTestData.CreateValidBooking(
            start: baseTime.AddHours(1), end: baseTime.AddHours(3), roomNumber: 2);

        var errors = await service.ValidateAsync(overlapping, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.Start) && e.Message.Contains("already booked"));
    }

    [Fact]
    public async Task ValidateAsync_ExcludesOwnBookingWhenUpdating()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var baseTime = DateTime.UtcNow.AddDays(1);
        await BookingTestData.SeedRoomAsync(context, 2);
        var existing = BookingTestData.CreateValidBooking(
            start: baseTime, end: baseTime.AddHours(2), roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, existing);

        var errors = await service.ValidateAsync(existing, excludeBookingId: existing.Id, now: DateTime.UtcNow);

        Assert.DoesNotContain(errors, e => e.Message.Contains("already booked"));
    }

    [Fact]
    public async Task ValidateAsync_ReturnsMultipleErrorsSimultaneously()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var futureDate = DateTime.UtcNow.AddDays(1);
        var booking = BookingTestData.CreateValidBooking(
            start: futureDate, end: futureDate, roomNumber: 999);

        var errors = await service.ValidateAsync(booking, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Field == nameof(Booking.End));
        Assert.Contains(errors, e => e.Field == nameof(Booking.RoomNumber));
    }

    [Fact]
    public async Task ValidateAsync_ExactBoundaryStartEqualsOtherEnd_NoOverlap()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var baseTime = DateTime.UtcNow.AddDays(1);
        await BookingTestData.SeedRoomAsync(context, 2);
        var existing = BookingTestData.CreateValidBooking(start: baseTime, end: baseTime.AddHours(1), roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, existing);

        var adjacent = BookingTestData.CreateValidBooking(
            start: baseTime.AddHours(1), end: baseTime.AddHours(2), roomNumber: 2);

        var errors = await service.ValidateAsync(adjacent, now: DateTime.UtcNow);

        Assert.DoesNotContain(errors, e => e.Message.Contains("already booked"));
    }

    [Fact]
    public async Task ValidateAsync_BookingFullyContainedInExisting_DetectsOverlap()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var baseTime = DateTime.UtcNow.AddDays(1);
        await BookingTestData.SeedRoomAsync(context, 2);
        var existing = BookingTestData.CreateValidBooking(start: baseTime, end: baseTime.AddHours(3), roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, existing);

        var contained = BookingTestData.CreateValidBooking(
            start: baseTime.AddHours(1), end: baseTime.AddHours(2), roomNumber: 2);

        var errors = await service.ValidateAsync(contained, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Message.Contains("already booked"));
    }

    [Fact]
    public async Task ValidateAsync_ExistingBookingFullyInsideNew_DetectsOverlap()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        var baseTime = DateTime.UtcNow.AddDays(1);
        await BookingTestData.SeedRoomAsync(context, 2);
        var existing = BookingTestData.CreateValidBooking(start: baseTime.AddHours(1), end: baseTime.AddHours(2), roomNumber: 2);
        await BookingTestData.SeedBookingAsync(context, existing);

        var enclosing = BookingTestData.CreateValidBooking(
            start: baseTime, end: baseTime.AddHours(3), roomNumber: 2);

        var errors = await service.ValidateAsync(enclosing, now: DateTime.UtcNow);

        Assert.Contains(errors, e => e.Message.Contains("already booked"));
    }

    [Fact]
    public async Task ValidateAsync_UsesProvidedNowParameter()
    {
        await using var context = await TestDatabaseFactory.CreateCleanContextAsync();
        var service = new BookingValidationService(context);

        await BookingTestData.SeedRoomAsync(context);
        var referenceNow = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var futureRelativeToReference = referenceNow.AddHours(2);
        var booking = BookingTestData.CreateValidBooking(
            start: futureRelativeToReference, end: futureRelativeToReference.AddHours(1), roomNumber: 2);

        var errors = await service.ValidateAsync(booking, now: referenceNow);

        Assert.Empty(errors);
    }
}
