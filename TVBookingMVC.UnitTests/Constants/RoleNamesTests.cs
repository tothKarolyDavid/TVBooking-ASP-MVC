using TVBookingMVC.Constants;

namespace TVBookingMVC.UnitTests.Constants;

public sealed class RoleNamesTests
{
    [Fact]
    public void Admin_ReturnsExpectedRoleName()
    {
        Assert.Equal("Admin", RoleNames.Admin);
    }
}
