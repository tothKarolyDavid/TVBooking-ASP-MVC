using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using TVBookingMVC;

namespace TVBookingMVCSelenium;

public class SeleniumWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}

public class BookingIndexPageTests : IClassFixture<SeleniumWebApplicationFactory>
{
    private readonly SeleniumWebApplicationFactory _factory;
    private string _baseUrl = null!;

    public BookingIndexPageTests(SeleniumWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private IWebDriver CreateDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        return new ChromeDriver(options);
    }

    private string GetBaseUrl()
    {
        if (_baseUrl == null)
        {
            var server = _factory.Server;
            _baseUrl = server.BaseAddress.ToString().TrimEnd('/');
        }
        return _baseUrl;
    }

    [Fact]
    public void IndexBookingsTableHasRows()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl());

        var table = driver.FindElement(By.ClassName("table"));
        Assert.NotNull(table);

        var rows = table.FindElements(By.TagName("tr"));
        Assert.True(rows.Count >= 2, "Expected at least 2 table rows (header + data)");
    }

    [Fact]
    public void FilterBySingleAgeLimit_ReturnsOnlyMatchingRows()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl());

        driver.FindElement(By.XPath("//input[@value='Gyermekbarát program']")).Click();

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.Url.Contains("ageLimit=Gyermekbar%C3%A1t+program"));

        var rows = driver.FindElements(By.CssSelector(".table tbody tr"));

        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            Assert.True(cells.Count > 0, "Data row should have cells");
            Assert.Equal("Gyermekbarát program", cells[5].Text);
        }
    }

    [Fact]
    public void FilterByMultipleAgeLimits_ReturnsAllMatchingRows()
    {
        using var driver = CreateDriver();
        driver.Navigate().GoToUrl(GetBaseUrl());

        driver.FindElement(By.XPath("//input[@value='Gyermekbarát program']")).Click();

        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(driver, TimeSpan.FromSeconds(5));
        wait.Until(d => d.Url.Contains("ageLimit=Gyermekbar%C3%A1t+program"));

        driver.FindElement(By.XPath("//input[@value='Korhatárra való tekintet nélkül megtekinthető']")).Click();

        wait.Until(d => d.Url.Contains("ageLimit=Korhat%C3%A1rra+val%C3%B3+tekintet+n%C3%A9lk%C3%BCl+megtekinthet%C5%91"));

        var rows = driver.FindElements(By.CssSelector(".table tbody tr"));

        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            Assert.True(cells.Count > 0);
            var ageLimit = cells[5].Text;
            Assert.True(
                ageLimit == "Gyermekbarát program" ||
                ageLimit == "Korhatárra való tekintet nélkül megtekinthető",
                $"Unexpected age limit: {ageLimit}");
        }
    }
}