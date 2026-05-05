using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TVBookingMVC;

namespace TVBookingMVCSelenium;

public class BookingNavigationAndFilterTests : IClassFixture<SeleniumWebApplicationFactory>
{
    private readonly SeleniumWebApplicationFactory _factory;
    private string _baseUrl = null!;

    public BookingNavigationAndFilterTests(SeleniumWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return new ChromeDriver(options);
    }

    private string GetBaseUrl()
    {
        if (_baseUrl == null)
        {
            var server = _factory.Server;
            _baseUrl = server.BaseAddress.ToString().TrimEnd('/');
        }
        return _baseUrl;
    }

    [Fact]
    public void FreeTimeSlotsPageShowsTable()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl() + "/Booking/FreeTimeSlots");

        var table = driver.FindElement(By.ClassName("table"));
        var rows = table.FindElements(By.TagName("tr"));
        Assert.True(rows.Count >= 2, "Expected header + data rows");
    }

    [Fact]
    public void UserBookingsPageShowsOnlyCurrentRoomBookings()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl() + "/Identity/Account/Login");

        driver.FindElement(By.Id("email")).SendKeys("room2@hotel.com");
        driver.FindElement(By.Id("roomnumber")).SendKeys("2");
        driver.FindElement(By.Id("login-submit")).Click();

        driver.Navigate().GoToUrl(GetBaseUrl() + "/Booking/UserBookings");

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