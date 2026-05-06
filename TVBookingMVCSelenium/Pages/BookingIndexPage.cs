using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace TVBookingMVCSelenium.Pages;

public class BookingIndexPage
{
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;
    private readonly int _defaultTimeout;

    private static readonly By TableSelector = By.CssSelector("table.table");
    private static readonly By DataRowSelector = By.CssSelector("table.table tbody tr");
    private static readonly By HeaderRowSelector = By.CssSelector("table.table thead tr");

    public BookingIndexPage(IWebDriver driver, string baseUrl, int defaultTimeout = 5)
    {
        _driver = driver;
        _baseUrl = baseUrl;
        _defaultTimeout = defaultTimeout;
    }

    public void Navigate() => _driver.Navigate().GoToUrl(_baseUrl);

    public void NavigateWithMyBookings() =>
        _driver.Navigate().GoToUrl($"{_baseUrl}/Booking?myBookings=true");

    public IReadOnlyList<IWebElement> GetHeaderRows() =>
        _driver.FindElements(HeaderRowSelector);

    public IReadOnlyList<IWebElement> GetDataRows() =>
        _driver.FindElements(DataRowSelector);

    public IReadOnlyList<IWebElement> GetAllRows()
    {
        var table = _driver.FindElement(TableSelector);
        return table.FindElements(By.TagName("tr"));
    }

    public void ClickAgeLimitCheckbox(string value)
    {
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout));
        var checkbox = wait.Until(d => d.FindElement(By.XPath($"//input[@value='{value}']")));
        checkbox.Click();
    }

    public void WaitForUrlToContain(string text) =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => d.Url.Contains(text));

    public void WaitForPageLoad() =>
        new WebDriverWait(_driver, TimeSpan.FromSeconds(_defaultTimeout))
            .Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
}
