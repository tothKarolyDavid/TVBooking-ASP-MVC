using System.Xml.Linq;
using Moq;
using TVBookingMVC.Models;
using TVBookingMVC.Services;

namespace TVBookingMVC.UnitTests.Services;

public sealed class BookingExportServiceTests
{
    private static XDocument LoadXml(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        return XDocument.Load(stream);
    }

    [Fact]
    public async Task ExportBookingsToXmlAsync_WithBookings_ReturnsValidXml()
    {
        var date = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                Program = "Morning News",
                Channel = "BBC News",
                Genre = "News",
                Start = date.AddHours(8),
                End = date.AddHours(9),
                AgeLimit = "General Audience",
                RoomNumber = 2
            },
            new()
            {
                Id = 2,
                Program = "Afternoon Movie",
                Channel = "HBO",
                Genre = "Movie",
                Start = date.AddHours(14),
                End = date.AddHours(16),
                AgeLimit = "Under 12 not recommended",
                RoomNumber = 3
            }
        };

        var queryServiceMock = new Mock<IBookingQueryService>();
        queryServiceMock.Setup(q => q.GetBookingsForDateAsync(date)).ReturnsAsync(bookings);
        var service = new BookingExportService(queryServiceMock.Object);

        var result = await service.ExportBookingsToXmlAsync(date);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var xml = LoadXml(result);
        var root = xml.Root;
        Assert.NotNull(root);
        Assert.Equal("bookings", root.Name.LocalName);
        Assert.Equal(2, root.Elements("booking").Count());

        var firstBooking = root.Elements("booking").First();
        Assert.Equal("Morning News", firstBooking.Element("program")?.Value);
        Assert.Equal("BBC News", firstBooking.Element("channel")?.Value);
        Assert.Equal("News", firstBooking.Element("genre")?.Value);
        Assert.Equal("2025/06/01 08:00", firstBooking.Element("start")?.Value);
        Assert.Equal("2025/06/01 09:00", firstBooking.Element("end")?.Value);
        Assert.Equal("General Audience", firstBooking.Element("ageLimit")?.Value);
        Assert.Equal("2", firstBooking.Element("roomNumber")?.Value);
    }

    [Fact]
    public async Task ExportBookingsToXmlAsync_WithNoBookings_ReturnsEmptyBookingsXml()
    {
        var date = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var queryServiceMock = new Mock<IBookingQueryService>();
        queryServiceMock.Setup(q => q.GetBookingsForDateAsync(date)).ReturnsAsync(new List<Booking>());
        var service = new BookingExportService(queryServiceMock.Object);

        var result = await service.ExportBookingsToXmlAsync(date);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);

        var xml = LoadXml(result);
        Assert.NotNull(xml.Root);
        Assert.Equal("bookings", xml.Root.Name.LocalName);
        Assert.Empty(xml.Root.Elements("booking"));
    }

    [Fact]
    public async Task ExportBookingsToXmlAsync_UsesCorrectDateFormat()
    {
        var date = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                Program = "Test",
                Channel = "BBC News",
                Genre = "News",
                Start = date.AddHours(8).AddMinutes(5),
                End = date.AddHours(9).AddMinutes(15),
                AgeLimit = "General Audience",
                RoomNumber = 2
            }
        };

        var queryServiceMock = new Mock<IBookingQueryService>();
        queryServiceMock.Setup(q => q.GetBookingsForDateAsync(date)).ReturnsAsync(bookings);
        var service = new BookingExportService(queryServiceMock.Object);

        var result = await service.ExportBookingsToXmlAsync(date);
        var xml = LoadXml(result);

        var startElement = xml.Root!.Element("booking")!.Element("start")!.Value;
        var endElement = xml.Root!.Element("booking")!.Element("end")!.Value;

        Assert.Equal("2025/06/01 08:05", startElement);
        Assert.Equal("2025/06/01 09:15", endElement);
    }

    [Fact]
    public async Task ExportBookingsToXmlAsync_EncodesSpecialCharacters()
    {
        var date = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var bookings = new List<Booking>
        {
            new()
            {
                Id = 1,
                Program = "Show & Tell <Fun>",
                Channel = "M1",
                Genre = "News",
                Start = date.AddHours(8),
                End = date.AddHours(9),
                AgeLimit = "Children & Family",
                RoomNumber = 2
            }
        };

        var queryServiceMock = new Mock<IBookingQueryService>();
        queryServiceMock.Setup(q => q.GetBookingsForDateAsync(date)).ReturnsAsync(bookings);
        var service = new BookingExportService(queryServiceMock.Object);

        var result = await service.ExportBookingsToXmlAsync(date);
        var xml = LoadXml(result);

        var programElement = xml.Root!.Element("booking")!.Element("program")!.Value;
        var ageLimitElement = xml.Root!.Element("booking")!.Element("ageLimit")!.Value;

        Assert.Equal("Show & Tell <Fun>", programElement);
        Assert.Equal("Children & Family", ageLimitElement);
    }
}
