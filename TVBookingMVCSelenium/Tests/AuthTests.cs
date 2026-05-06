using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

[Collection("Selenium")]
public class AuthTests : TestBase
{
    public AuthTests(SeleniumWebApplicationFactory factory, WebDriverFixture driverFixture)
        : base(factory, driverFixture) { }

    [Theory]
    [InlineData("room2@hotel.com", "2", "room2@hotel.com")]
    [InlineData("admin@hotel.com", "0", "admin@hotel.com")]
    public void Login_ShowsCorrectHelloMessage(string email, string room, string expected)
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login(email, room);
        loginPage.GetManageText().Should().Be(expected);
    }

    [Fact]
    public void Logout_RedirectsToLoginPage()
    {
        var loginPage = new LoginPage(Driver, BaseUrl);
        loginPage.Login("room2@hotel.com", "2");
        loginPage.Logout();
        loginPage.IsLoginLinkVisible().Should().BeTrue();
    }
}
