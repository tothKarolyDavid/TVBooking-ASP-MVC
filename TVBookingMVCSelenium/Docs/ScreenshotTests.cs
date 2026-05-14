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
    private const string LightTheme = "light";
    private const string DarkTheme = "dark";

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

    private void SetThemePreference(string theme)
    {
        Driver.Navigate().GoToUrl(BaseUrl);
        ((IJavaScriptExecutor)Driver)
            .ExecuteScript("localStorage.setItem('tvbooking-theme-preference', arguments[0]);", theme);
    }

    private void CaptureWithTheme(string theme, string filename, Action captureAction)
    {
        SetThemePreference(theme);
        captureAction();
        TakeScreenshot(filename);
    }

    private void CaptureBothThemes(string baseName, Action captureAction)
    {
        CaptureWithTheme(LightTheme, $"{baseName}.light.png", captureAction);
        CaptureWithTheme(DarkTheme, $"{baseName}.dark.png", captureAction);
    }

    [Fact]
    public void Capture_01_BookingList()
    {
        CaptureBothThemes("01-booking-list", () =>
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            var indexPage = new BookingIndexPage(Driver, BaseUrl);
            indexPage.Navigate();
            indexPage.WaitForPageLoad();
            Thread.Sleep(500);
        });
    }

    [Fact]
    public void Capture_02_CreateBooking()
    {
        CaptureBothThemes("02-create-booking", () =>
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            var loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Login("room2@hotel.com", "2");

            var createPage = new CreateBookingPage(Driver, BaseUrl);
            createPage.Navigate();
            createPage.WaitForPageLoad();
            Thread.Sleep(500);
        });
    }

    [Fact]
    public void Capture_03_Statistics()
    {
        CaptureBothThemes("03-statistics", () =>
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            var loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Login("admin@hotel.com", "0");

            var statsPage = new StatisticsPage(Driver, BaseUrl);
            statsPage.Navigate();
            statsPage.WaitForPageLoad();
            statsPage.WaitForCharts();
        });
    }

    [Fact]
    public void Capture_04_GuestManagement()
    {
        CaptureBothThemes("04-guest-management", () =>
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            var loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Login("admin@hotel.com", "0");

            var guestsPage = new GuestsPage(Driver, BaseUrl);
            guestsPage.Navigate();
            guestsPage.WaitForPageLoad();
            Thread.Sleep(500);
        });
    }

    [Fact]
    public void Capture_05_Login()
    {
        CaptureBothThemes("05-login", () =>
        {
            Driver.Manage().Cookies.DeleteAllCookies();
            var loginPage = new LoginPage(Driver, BaseUrl);
            loginPage.Navigate();
            loginPage.WaitForPageLoad();
            Thread.Sleep(300);
        });
    }
}
