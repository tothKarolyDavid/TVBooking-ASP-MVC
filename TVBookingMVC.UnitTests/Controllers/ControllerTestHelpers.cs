using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TVBookingMVC.Areas.Identity.Data;

namespace TVBookingMVC.UnitTests.Controllers;

internal static class ControllerTestHelpers
{
    public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static Mock<SignInManager<ApplicationUser>> CreateMockSignInManager(Mock<UserManager<ApplicationUser>> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        return new Mock<SignInManager<ApplicationUser>>(
            userManager.Object, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);
    }

    public static ControllerContext CreateControllerContext(string? username = "user@test.com", bool isAuthenticated = true, string? role = null)
    {
        var claims = new List<Claim>();
        if (isAuthenticated && username != null)
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }
        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(claims, isAuthenticated ? "mock" : null);
        var user = new ClaimsPrincipal(identity);
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    public static void SetupUserManagerGetUser(Mock<UserManager<ApplicationUser>> userManager, ApplicationUser? user)
    {
        userManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
    }

    public static void SetupUserManagerFindByName(Mock<UserManager<ApplicationUser>> userManager, ApplicationUser? user)
    {
        userManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
    }
}
