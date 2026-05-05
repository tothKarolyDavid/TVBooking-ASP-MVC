using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TVBookingMVCSelenium;

public class BookingNavigationAndFilterTests
{
    private const string BaseUrl = "http://localhost:7233/";

    private static IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        return new ChromeDriver(options);
    }

    [Fact]
    public void FreeTimeSlotsPageShowsTable()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(BaseUrl + "Booking/FreeTimeSlots");

        var table = driver.FindElement(By.ClassName("table"));
        var rows = table.FindElements(By.TagName("tr"));
        Assert.True(rows.Count >= 2, "Expected header + data rows");
    }

    [Fact]
    public void UserBookingsPageShowsOnlyCurrentRoomBookings()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(BaseUrl + "Identity/Account/Login");

        driver.FindElement(By.Id("email")).SendKeys("room2@hotel.com");
        driver.FindElement(By.Id("roomnumber")).SendKeys("2");
        driver.FindElement(By.Id("login-submit")).Click();

        driver.Navigate().GoToUrl(BaseUrl + "Booking/UserBookings");

        var rows = driver.FindElements(By.CssSelector(".table tbody tr"));

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