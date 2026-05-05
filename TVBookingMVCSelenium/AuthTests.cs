using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TVBookingMVCSelenium;

public class AuthTests
{
    private const string BaseUrl = "http://localhost:7233/";

    private static IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        return new ChromeDriver(options);
    }

    [Fact]
    public void LoginAsRoom2Guest_ShowsCorrectHelloMessage()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(BaseUrl + "Identity/Account/Login");

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
        driver.Navigate().GoToUrl(BaseUrl + "Identity/Account/Login");

        driver.FindElement(By.Id("email")).SendKeys("room2@hotel.com");
        driver.FindElement(By.Id("roomnumber")).SendKeys("2");
        driver.FindElement(By.Id("login-submit")).Click();

        driver.FindElement(By.Id("logout")).Click();

        var loginButton = driver.FindElement(By.Id("login"));
        Assert.NotNull(loginButton);
    }
}