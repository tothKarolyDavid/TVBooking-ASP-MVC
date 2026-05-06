using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TVBookingMVCSelenium.Infrastructure;

public abstract class TestBase : IClassFixture<SeleniumWebApplicationFactory>
{
    private readonly SeleniumWebApplicationFactory _factory;
    private string _baseUrl = null!;

    protected TestBase(SeleniumWebApplicationFactory factory)
    {
        _factory = factory;
    }

    protected string BaseUrl
    {
        get
        {
            if (_baseUrl == null)
            {
                var server = _factory.Server;
                _baseUrl = server.BaseAddress.ToString().TrimEnd('/');
            }
            return _baseUrl;
        }
    }

    protected static IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return new ChromeDriver(options);
    }
}
