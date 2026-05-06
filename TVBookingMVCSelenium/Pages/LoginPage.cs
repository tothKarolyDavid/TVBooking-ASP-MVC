using OpenQA.Selenium;

namespace TVBookingMVCSelenium.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public LoginPage(IWebDriver driver, string baseUrl)
    {
        _driver = driver;
        _baseUrl = baseUrl;
    }

    public void Navigate()
    {
        _driver.Navigate().GoToUrl(_baseUrl + "/Identity/Account/Login");
    }

    public void Login(string email, string roomNumber)
    {
        Navigate();
        _driver.FindElement(By.Id("email")).SendKeys(email);
        _driver.FindElement(By.Id("roomnumber")).SendKeys(roomNumber);
        _driver.FindElement(By.Id("login-submit")).Click();
    }

    public void Logout()
    {
        _driver.FindElement(By.Id("logout")).Click();
    }

    public string GetManageText()
    {
        return _driver.FindElement(By.Id("manage")).Text;
    }

    public IWebElement? GetLoginButton()
    {
        return _driver.FindElements(By.Id("login")).FirstOrDefault();
    }
}
