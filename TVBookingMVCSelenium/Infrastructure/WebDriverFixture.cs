using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TVBookingMVCSelenium.Infrastructure;

public class WebDriverFixture : IDisposable
{
    public IWebDriver Driver { get; }

    private static readonly string DownloadedChrome = "/tmp/chromium/chrome-linux64/chrome";
    private static readonly string DownloadedChromeDriver = "/tmp/chromium/chromedriver-linux64/chromedriver";

    public WebDriverFixture()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        if (File.Exists(DownloadedChrome))
        {
            options.BinaryLocation = DownloadedChrome;
            var driverDir = Path.GetDirectoryName(DownloadedChromeDriver)!;
            var service = ChromeDriverService.CreateDefaultService(driverDir);
            Driver = new ChromeDriver(service, options);
        }
        else
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            Driver = new ChromeDriver(options);
        }
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
