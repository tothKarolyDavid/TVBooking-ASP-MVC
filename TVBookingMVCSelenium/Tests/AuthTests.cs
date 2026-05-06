using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

public class AuthTests : TestBase
{
    public AuthTests(SeleniumWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public void LoginAsRoom2Guest_ShowsCorrectHelloMessage()
    {
        using var driver = CreateDriver();
        var loginPage = new LoginPage(driver, BaseUrl);

        loginPage.Login("room2@hotel.com", "2");

        var manageText = loginPage.GetManageText();
        Assert.Contains("Hello room2@hotel", manageText);
    }

    [Fact]
    public void Logout_RedirectsToLoginPage()
    {
        using var driver = CreateDriver();
        var loginPage = new LoginPage(driver, BaseUrl);

        loginPage.Login("room2@hotel.com", "2");
        loginPage.Logout();

        var loginButton = loginPage.GetLoginButton();
        Assert.NotNull(loginButton);
    }
}
