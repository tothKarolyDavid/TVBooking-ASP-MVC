using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly int _defaultTimeout;

    private static readonly By EmailInput = By.Id("email");
    private static readonly By RoomNumberInput = By.Id("roomnumber");
    private static readonly By SubmitButton = By.Id("login-submit");
    private static readonly By LogoutButton = By.Id("logout");
    private static readonly By ManageLink = By.Id("manage");
    private static readonly By LoginNavLink = By.Id("login");

    public LoginPage(IWebDriver driver, string baseUrl, int defaultTimeout = 5)
    {
        _driver = driver;
        _baseUrl = baseUrl;
        _defaultTimeout = defaultTimeout;
    }

    public void Navigate() =>
        _driver.Navigate().GoToUrl($"{_baseUrl}/Identity/Account/Login");

    public void WaitForPageLoad() =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

    public void EnterCredentials(string email, string roomNumber)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        wait.Until(d =>
        {
            d.FindElement(EmailInput).SendKeys(email);
            d.FindElement(RoomNumberInput).SendKeys(roomNumber);
            return true;
        });
    }

    public void Submit()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        wait.Until(d =>
        {
            d.FindElement(SubmitButton).Click();
            return true;
        });
    }

    public void Login(string email, string roomNumber)
    {
        _driver.Manage().Cookies.DeleteAllCookies();
        Navigate();
        WaitForPageLoad();
        EnterCredentials(email, roomNumber);
        Submit();
    }

    public void Logout()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        wait.Until(d =>
        {
            d.FindElement(LogoutButton).Click();
            return true;
        });
    }

    public string GetManageText()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait.Until(d => d.FindElement(ManageLink).Text);
    }

    public bool IsLoginLinkVisible()
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        return wait.Until(d => d.FindElements(LoginNavLink).Any());
    }
}
