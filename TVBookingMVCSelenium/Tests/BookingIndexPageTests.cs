using OpenQA.Selenium;
using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

public class BookingIndexPageTests : TestBase
{
    public BookingIndexPageTests(SeleniumWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public void IndexBookingsTableHasRows()
    {
        using var driver = CreateDriver();
        var page = new BookingIndexPage(driver, BaseUrl);

        page.Navigate();

        var rows = page.GetTableHeaderAndRows();
        Assert.True(rows.Count >= 2, "Expected at least 2 table rows (header + data)");
    }

    [Fact]
    public void FilterBySingleAgeLimit_ReturnsOnlyMatchingRows()
    {
        using var driver = CreateDriver();
        var page = new BookingIndexPage(driver, BaseUrl);

        page.Navigate();
        page.ClickAgeLimitCheckbox("Gyermekbarát program");
        page.WaitForUrlToContain("ageLimit=Gyermekbar%C3%A1t+program");

        var rows = page.GetTableRows();
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
        var page = new BookingIndexPage(driver, BaseUrl);

        page.Navigate();
        page.ClickAgeLimitCheckbox("Gyermekbarát program");
        page.WaitForUrlToContain("ageLimit=Gyermekbar%C3%A1t+program");

        page.ClickAgeLimitCheckbox("Korhatárra való tekintet nélkül megtekinthető");
        page.WaitForUrlToContain("ageLimit=Korhat%C3%A1rra+val%C3%B3+tekintet+n%C3%A9lk%C3%BCl+megtekinthet%C5%91");

        var rows = page.GetTableRows();
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
