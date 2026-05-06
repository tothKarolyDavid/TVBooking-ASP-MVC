using OpenQA.Selenium;

namespace TVBookingMVCSelenium.Infrastructure;

public abstract class TestBase : IClassFixture<WebDriverFixture>
{
    protected TestBase(SeleniumWebApplicationFactory factory, WebDriverFixture driverFixture)
    {
        BaseUrl = factory.BaseUrl;
        Driver = driverFixture.Driver;
    }

    protected IWebDriver Driver { get; }
    protected string BaseUrl { get; }
    protected int DefaultTimeoutSeconds => 5;
}
