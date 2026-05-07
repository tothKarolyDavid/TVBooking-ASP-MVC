using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Docs;

[Collection("Selenium")]
public class ScreenshotTests : TestBase
{
    private static readonly string ScreenshotDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Docs", "preview"));

    public ScreenshotTests(SeleniumWebApplicationFactory factory, WebDriverFixture driverFixture)
        : base(factory, driverFixture)
    {
        Driver.Manage().Window.Size = new System.Drawing.Size(1920, 1080);
        Directory.CreateDirectory(ScreenshotDir);
    }

    private void TakeScreenshot(string filename)
    {
        var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
        var filePath = Path.Combine(ScreenshotDir, filename);
        screenshot.SaveAsFile(filePath);
    }

    [Fact]
    public void Capture_01_BookingList()
    {
        Driver.Manage().Cookies.DeleteAllCookies();
        var indexPage = new BookingIndexPage(Driver, BaseUrl);
        indexPage.Navigate();
        indexPage.WaitForPageLoad();
        Thread.Sleep(500);
        TakeScreenshot("01-booking-list.png");
    }

    [Fact]
    public void Capture_02_CreateBooking()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login("room2@hotel.com", "2");

        var createPage = new CreateBookingPage(Driver, BaseUrl);
        createPage.Navigate();
        createPage.WaitForPageLoad();
        Thread.Sleep(500);
        TakeScreenshot("02-create-booking.png");
    }

    [Fact]
    public void Capture_03_Statistics()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login("admin@hotel.com", "0");

        var statsPage = new StatisticsPage(Driver, BaseUrl);
        statsPage.Navigate();
        statsPage.WaitForPageLoad();
        statsPage.WaitForCharts();
        TakeScreenshot("03-statistics.png");
    }

    [Fact]
    public void Capture_04_GuestManagement()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login("admin@hotel.com", "0");

        var guestsPage = new GuestsPage(Driver, BaseUrl);
        guestsPage.Navigate();
        guestsPage.WaitForPageLoad();
        Thread.Sleep(500);
        TakeScreenshot("04-guest-management.png");
    }

    [Fact]
    public void Capture_05_Login()
    {
        Driver.Manage().Cookies.DeleteAllCookies();
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Navigate();
        loginPage.WaitForPageLoad();
        Thread.Sleep(300);
        TakeScreenshot("05-login.png");
    }
}
