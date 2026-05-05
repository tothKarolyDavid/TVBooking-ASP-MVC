using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TVBookingMVC;

namespace TVBookingMVCSelenium;

public class AuthTests : IClassFixture<SeleniumWebApplicationFactory>
{
    private readonly SeleniumWebApplicationFactory _factory;
    private string _baseUrl = null!;

    public AuthTests(SeleniumWebApplicationFactory factory)
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
    public void LoginAsRoom2Guest_ShowsCorrectHelloMessage()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl() + "/Identity/Account/Login");

        driver.FindElement(By.Id("email")).SendKeys("room2@hotel.com");
        driver.FindElement(By.Id("roomnumber")).SendKeys("2");
        driver.FindElement(By.Id("login-submit")).Click();

        var manageText = driver.FindElement(By.Id("manage")).Text;
        Assert.Contains("Hello room2@hotel", manageText);
    }

    [Fact]
    public void Logout_RedirectsToLoginPage()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl() + "/Identity/Account/Login");

        driver.FindElement(By.Id("email")).SendKeys("room2@hotel.com");
        driver.FindElement(By.Id("roomnumber")).SendKeys("2");
        driver.FindElement(By.Id("login-submit")).Click();

        driver.FindElement(By.Id("logout")).Click();

        var loginButton = driver.FindElement(By.Id("login"));
        Assert.NotNull(loginButton);
    }
}