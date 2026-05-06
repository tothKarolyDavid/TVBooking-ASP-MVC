using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class BookingIndexPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public BookingIndexPage(IWebDriver driver, string baseUrl)
    {
        _driver = driver;
        _baseUrl = baseUrl;
    }

    public void Navigate()
    {
        _driver.Navigate().GoToUrl(_baseUrl);
    }

    public void NavigateWithMyBookings()
    {
        _driver.Navigate().GoToUrl(_baseUrl + "/Booking?myBookings=true");
    }

    public IWebElement GetTable()
    {
        return _driver.FindElement(By.ClassName("table"));
    }

    public List<IWebElement> GetTableRows()
    {
        return [.. _driver.FindElements(By.CssSelector(".table tbody tr"))];
    }

    public List<IWebElement> GetTableHeaderAndRows()
    {
        var table = GetTable();
        return [.. table.FindElements(By.TagName("tr"))];
    }

    public void ClickAgeLimitCheckbox(string value)
    {
        _driver.FindElement(By.XPath($"//input[@value='{value}']")).Click();
    }

    public void WaitForUrlToContain(string text, int timeoutSeconds = 5)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d => d.Url.Contains(text));
    }
}
