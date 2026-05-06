using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TVBookingMVCSelenium.Infrastructure;

public class WebDriverFixture : IDisposable
{
    public IWebDriver Driver { get; }

    public WebDriverFixture()
    {
        new DriverManager().SetUpDriver(new ChromeConfig());
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        Driver = new ChromeDriver(options);
    }

    public void Dispose()
    {
        Driver.Quit();
        Driver.Dispose();
    }
}
