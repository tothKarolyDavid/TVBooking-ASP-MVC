using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class CreateBookingPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly int _defaultTimeout;

    public CreateBookingPage(IWebDriver driver, string baseUrl, int defaultTimeout = 5)
    {
        _driver = driver;
        _baseUrl = baseUrl;
        _defaultTimeout = defaultTimeout;
    }

    public void Navigate() =>
        _driver.Navigate().GoToUrl($"{_baseUrl}/Booking/Create");

    public void WaitForPageLoad() =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

    public IWebElement? GetFreeSlotsTable()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
            return wait.Until(d => d.FindElement(By.Id("freeSlotsTable")));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }
}
