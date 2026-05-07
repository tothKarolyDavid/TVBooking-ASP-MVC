using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class GuestsPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly int _defaultTimeout;

    public GuestsPage(IWebDriver driver, string baseUrl, int defaultTimeout = 5)
    {
        _driver = driver;
        _baseUrl = baseUrl;
        _defaultTimeout = defaultTimeout;
    }

    public void Navigate() =>
        _driver.Navigate().GoToUrl($"{_baseUrl}/Guests");

    public void WaitForPageLoad() =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
}
