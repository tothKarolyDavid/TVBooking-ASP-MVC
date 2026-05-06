using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Controllers;
using TVBookingMVC.Services;

namespace TVBookingMVC.UnitTests.Controllers;

public sealed class AccountControllerTests
{
    [Fact]
    public async Task Checkout_WhenGuest_DeletesBookingsAndUserAndSignsOut()
    {
        var user = new ApplicationUser { Id = "1", UserName = "guest@hotel.com", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerGetUser(userManager, user);
        userManager.Setup(m => m.IsInRoleAsync(user, RoleNames.Admin)).ReturnsAsync(false);
        userManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var commandService = new Mock<IBookingCommandService>();
        var signInManager = ControllerTestHelpers.CreateMockSignInManager(userManager);

        var controller = new AccountController(commandService.Object, userManager.Object, signInManager.Object);
        controller.ControllerContext = ControllerTestHelpers.CreateControllerContext("guest@hotel.com");

        var result = await controller.Checkout();

        commandService.Verify(s => s.DeleteBookingsByRoomAsync(5), Times.Once);
        userManager.Verify(m => m.DeleteAsync(user), Times.Once);
        signInManager.Verify(s => s.SignOutAsync(), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Booking", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Checkout_WhenAdmin_RedirectsWithoutDeleting()
    {
        var user = new ApplicationUser { Id = "1", UserName = "admin@hotel.com", RoomNumber = 0 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerGetUser(userManager, user);
        userManager.Setup(m => m.IsInRoleAsync(user, RoleNames.Admin)).ReturnsAsync(true);

        var commandService = new Mock<IBookingCommandService>();
        var signInManager = ControllerTestHelpers.CreateMockSignInManager(userManager);

        var controller = new AccountController(commandService.Object, userManager.Object, signInManager.Object);
        controller.ControllerContext = ControllerTestHelpers.CreateControllerContext("admin@hotel.com");

        var result = await controller.Checkout();

        commandService.Verify(s => s.DeleteBookingsByRoomAsync(It.IsAny<int>()), Times.Never);
        userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Booking", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Checkout_WhenUnauthenticated_Redirects()
    {
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

        var commandService = new Mock<IBookingCommandService>();
        var signInManager = ControllerTestHelpers.CreateMockSignInManager(userManager);

        var controller = new AccountController(commandService.Object, userManager.Object, signInManager.Object);
        controller.ControllerContext = ControllerTestHelpers.CreateControllerContext("guest@hotel.com");

        var result = await controller.Checkout();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Booking", redirect.ControllerName);
        Assert.Equal("Index", redirect.ActionName);
    }
}
