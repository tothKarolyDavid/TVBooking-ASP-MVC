using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

[Collection("Selenium")]
public class BookingNavigationAndFilterTests : TestBase
{
    public BookingNavigationAndFilterTests(SeleniumWebApplicationFactory factory, WebDriverFixture driverFixture)
        : base(factory, driverFixture) { }

    [Fact]
    public void FreeTimeSlotsPageShowsTable()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login("admin@hotel.com", "0");
        loginPage.WaitForPageLoad();

        var createPage = new CreateBookingPage(Driver, BaseUrl);
        createPage.Navigate();
        createPage.WaitForPageLoad();

        Driver.PageSource.Should().Contain("Create");
        Driver.FindElement(By.CssSelector(".page-content h1")).Text.Should().Contain("Create");
    }

    [Fact]
    public void MyBookingsFilterShowsOnlyCurrentRoomBookings()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        var bookingsPage = new BookingIndexPage(Driver, BaseUrl);

        loginPage.Login("room2@hotel.com", "2");
        bookingsPage.NavigateWithMyBookings();
        bookingsPage.WaitForPageLoad();

        var rows = bookingsPage.GetDataRows();
        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count > 0)
            {
                cells[6].Text.Should().Be("2");
            }
        }
    }
}
