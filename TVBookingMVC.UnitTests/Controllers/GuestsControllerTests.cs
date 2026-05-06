using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Controllers;
using TVBookingMVC.Services;
using TVBookingMVC.ViewModels;

namespace TVBookingMVC.UnitTests.Controllers;

public sealed class GuestsControllerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ApplicationDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 0;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();
        _dbContext = _provider.GetRequiredService<ApplicationDbContext>();
        await _dbContext.Database.OpenConnectionAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _provider.DisposeAsync();
    }

    private async Task<(GuestsController Controller, UserManager<ApplicationUser> UserManager, Mock<IBookingCommandService> CommandService)> CreateControllerAsync()
    {
        var userManager = _provider.GetRequiredService<UserManager<ApplicationUser>>();
        var commandService = new Mock<IBookingCommandService>();
        var logger = _provider.GetRequiredService<ILogger<GuestsController>>();

        var controller = new GuestsController(userManager, commandService.Object, logger);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return (controller, userManager, commandService);
    }

    [Fact]
    public async Task Index_ReturnsViewWithViewModel()
    {
        var (controller, userManager, _) = await CreateControllerAsync();

        await userManager.CreateAsync(new ApplicationUser
        {
            UserName = "room2@hotel.com",
            Email = "room2@hotel.com",
            RoomNumber = 2
        });
        await userManager.CreateAsync(new ApplicationUser
        {
            UserName = "room5@hotel.com",
            Email = "room5@hotel.com",
            RoomNumber = 5
        });

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ManageGuestsViewModel>(viewResult.Model);
        Assert.Equal(2, vm.Users.Count);
        Assert.DoesNotContain(vm.AvailableRooms, r => r == 2 || r == 5);
    }

    [Fact]
    public async Task Delete_WhenGuestExists_DeletesBookingsAndUser()
    {
        var (controller, userManager, commandService) = await CreateControllerAsync();

        var user = new ApplicationUser
        {
            UserName = "room2@hotel.com",
            Email = "room2@hotel.com",
            RoomNumber = 2
        };
        var createResult = await userManager.CreateAsync(user);
        Assert.True(createResult.Succeeded);

        var result = await controller.Delete(user.Id);

        commandService.Verify(s => s.DeleteBookingsByRoomAsync(2), Times.Once);
        var deleted = await userManager.FindByIdAsync(user.Id);
        Assert.Null(deleted);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_WhenUserIsAdmin_DoesNotDelete()
    {
        var (controller, userManager, commandService) = await CreateControllerAsync();

        var user = new ApplicationUser
        {
            UserName = "admin@hotel.com",
            Email = "admin@hotel.com",
            RoomNumber = 0
        };
        var createResult = await userManager.CreateAsync(user);
        Assert.True(createResult.Succeeded);

        var adminRole = new IdentityRole(RoleNames.Admin);
        var roleManager = _provider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.CreateAsync(adminRole);
        await userManager.AddToRoleAsync(user, RoleNames.Admin);

        var result = await controller.Delete(user.Id);

        commandService.Verify(s => s.DeleteBookingsByRoomAsync(It.IsAny<int>()), Times.Never);
        var stillExists = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(stillExists);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_WhenUserNotFound_ReturnsNotFound()
    {
        var (controller, _, _) = await CreateControllerAsync();

        var result = await controller.Delete("nonexistent-id");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AssignRoom_WhenRoomAvailable_UpdatesUserAndReassignsBookings()
    {
        var (controller, userManager, commandService) = await CreateControllerAsync();

        var user = new ApplicationUser
        {
            UserName = "room2@hotel.com",
            Email = "room2@hotel.com",
            RoomNumber = 2
        };
        var createResult = await userManager.CreateAsync(user);
        Assert.True(createResult.Succeeded);

        var result = await controller.AssignRoom(user.Id, 10);

        Assert.Equal(10, user.RoomNumber);
        commandService.Verify(s => s.ReassignBookingsByRoomAsync(2, 10), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task AssignRoom_WhenRoomOccupied_DoesNotUpdate()
    {
        var (controller, userManager, commandService) = await CreateControllerAsync();

        var user = new ApplicationUser
        {
            UserName = "room2@hotel.com",
            Email = "room2@hotel.com",
            RoomNumber = 2
        };
        var occupant = new ApplicationUser
        {
            UserName = "room5@hotel.com",
            Email = "room5@hotel.com",
            RoomNumber = 5
        };
        await userManager.CreateAsync(user);
        await userManager.CreateAsync(occupant);

        var result = await controller.AssignRoom(user.Id, 5);

        commandService.Verify(s => s.ReassignBookingsByRoomAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
}
