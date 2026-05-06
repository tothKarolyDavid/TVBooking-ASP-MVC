using OpenQA.Selenium;
using TVBookingMVCSelenium.Infrastructure;
using TVBookingMVCSelenium.Pages;

namespace TVBookingMVCSelenium.Tests;

[Collection("Selenium")]
public class BookingIndexPageTests : TestBase
{
    public BookingIndexPageTests(SeleniumWebApplicationFactory factory, WebDriverFixture driverFixture)
        : base(factory, driverFixture) { }

    [Fact]
    public void IndexBookingsTableHasRows()
    {
        var page = new BookingIndexPage(Driver, BaseUrl);
        page.Navigate();
        page.WaitForPageLoad();
        var rows = page.GetAllRows();
        rows.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public void FilterBySingleAgeLimit_ReturnsOnlyMatchingRows()
    {
        var page = new BookingIndexPage(Driver, BaseUrl);
        page.Navigate();
        page.WaitForPageLoad();
        page.ClickAgeLimitCheckbox("Child-friendly Program");
        page.WaitForUrlToContain("ageLimit=Child-friendly+Program");

        var rows = page.GetDataRows();
        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            cells.Should().NotBeEmpty();
            cells[5].Text.Should().Be("Child-friendly Program");
        }
    }

    [Fact]
    public void FilterByMultipleAgeLimits_ReturnsAllMatchingRows()
    {
        var page = new BookingIndexPage(Driver, BaseUrl);
        page.Navigate();
        page.WaitForPageLoad();
        page.ClickAgeLimitCheckbox("Child-friendly Program");
        page.WaitForUrlToContain("ageLimit=Child-friendly+Program");

        page.ClickAgeLimitCheckbox("General Audience");
        page.WaitForUrlToContain("ageLimit=General+Audience");

        var rows = page.GetDataRows();
        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            cells.Should().NotBeEmpty();
            var ageLimit = cells[5].Text;
            ageLimit.Should().BeOneOf("Child-friendly Program", "General Audience");
        }
    }
}
