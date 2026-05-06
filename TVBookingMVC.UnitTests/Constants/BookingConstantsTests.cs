using TVBookingMVC.Constants;

namespace TVBookingMVC.UnitTests.Constants;

public sealed class BookingConstantsTests
{
    [Fact]
    public void DefaultBookingDurationHours_IsOne()
    {
        Assert.Equal(1, BookingConstants.DefaultBookingDurationHours);
    }

    [Fact]
    public void NearBookingWindowHours_Is24()
    {
        Assert.Equal(24, BookingConstants.NearBookingWindowHours);
    }

    [Fact]
    public void FreeSlotLookAheadDays_Is7()
    {
        Assert.Equal(7, BookingConstants.FreeSlotLookAheadDays);
    }

    [Fact]
    public void DefaultStatisticsWindowDays_Is30()
    {
        Assert.Equal(30, BookingConstants.DefaultStatisticsWindowDays);
    }

    [Fact]
    public void MaxRoomNumber_Is999()
    {
        Assert.Equal(999, BookingConstants.MaxRoomNumber);
    }

    [Fact]
    public void MinRoomNumber_Is1()
    {
        Assert.Equal(1, BookingConstants.MinRoomNumber);
    }

    [Fact]
    public void AdminRoomNumber_Is0()
    {
        Assert.Equal(0, BookingConstants.AdminRoomNumber);
    }
}
