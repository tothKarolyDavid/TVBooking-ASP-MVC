using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class StatisticsPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly int _defaultTimeout;

    public StatisticsPage(IWebDriver driver, string baseUrl, int defaultTimeout = 5)
    {
        _driver = driver;
        _baseUrl = baseUrl;
        _defaultTimeout = defaultTimeout;
    }

    public void Navigate() =>
        _driver.Navigate().GoToUrl($"{_baseUrl}/Booking/Statistics");

    public void WaitForPageLoad() =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

    public void WaitForCharts()
    {
        Thread.Sleep(2000); // Give CDN time to load Chart.js
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout * 3));
        wait.Until(d =>
        {
            try
            {
                var scriptResult = ((IJavaScriptExecutor)d).ExecuteScript(
                    "return typeof Chart !== 'undefined' && document.querySelectorAll('canvas').length > 0");
                return scriptResult is true;
            }
            catch
            {
                return false;
            }
        });
        Thread.Sleep(1000);
    }
}
