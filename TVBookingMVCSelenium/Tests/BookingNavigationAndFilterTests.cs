using OpenQA.Selenium;
using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

public class BookingNavigationAndFilterTests : TestBase
{
    public BookingNavigationAndFilterTests(SeleniumWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public void FreeTimeSlotsPageShowsTable()
    {
        using var driver = CreateDriver();
        var page = new BookingIndexPage(driver, BaseUrl);

        driver.Navigate().GoToUrl(BaseUrl + "/Booking/FreeTimeSlots");

        var rows = page.GetTableHeaderAndRows();
        Assert.True(rows.Count >= 2, "Expected header + data rows");
    }

    [Fact]
    public void MyBookingsFilterShowsOnlyCurrentRoomBookings()
    {
        using var driver = CreateDriver();
        var loginPage = new LoginPage(driver, BaseUrl);
        var bookingsPage = new BookingIndexPage(driver, BaseUrl);

        loginPage.Login("room2@hotel.com", "2");
        bookingsPage.NavigateWithMyBookings();

        var rows = bookingsPage.GetTableRows();
        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count > 0)
            {
                Assert.Equal("2", cells[6].Text);
            }
        }
    }
}
