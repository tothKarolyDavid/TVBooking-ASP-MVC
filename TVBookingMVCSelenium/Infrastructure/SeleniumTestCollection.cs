namespace TVBookingMVCSelenium.Infrastructure;

[CollectionDefinition("Selenium", DisableParallelization = true)]
public class SeleniumTestCollection : ICollectionFixture<SeleniumWebApplicationFactory>
{
}
