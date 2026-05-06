using System.Xml.Linq;
using TVBookingMVC.Models;

namespace TVBookingMVC.Services;

public interface IBookingExportService
{
    Task<byte[]> ExportBookingsToXmlAsync(DateTime date);
}

public sealed class BookingExportService : IBookingExportService
{
    private readonly IBookingQueryService _bookingQueryService;

    public BookingExportService(IBookingQueryService bookingQueryService)
    {
        _bookingQueryService = bookingQueryService;
    }

    public async Task<byte[]> ExportBookingsToXmlAsync(DateTime date)
    {
        var bookings = await _bookingQueryService.GetBookingsForDateAsync(date);

        var doc = new XDocument(
            new XElement("bookings",
                bookings.Select(b =>
                    new XElement("booking",
                        new XElement("program", b.Program),
                        new XElement("channel", b.Channel),
                        new XElement("genre", b.Genre),
                        new XElement("start", b.Start.ToString("yyyy/MM/dd HH:mm")),
                        new XElement("end", b.End.ToString("yyyy/MM/dd HH:mm")),
                        new XElement("ageLimit", b.AgeLimit),
                        new XElement("roomNumber", b.RoomNumber)
                    )
                )
            )
        );

        using var stream = new MemoryStream();
        doc.Save(stream);
        return stream.ToArray();
    }
}
