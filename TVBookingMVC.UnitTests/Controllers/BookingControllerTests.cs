using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Controllers;
using TVBookingMVC.Models;
using TVBookingMVC.Services;
using TVBookingMVC.ViewModels;

namespace TVBookingMVC.UnitTests.Controllers;

public sealed class BookingControllerTests
{
    private readonly Mock<IBookingQueryService> _queryService = new();
    private readonly Mock<IBookingCommandService> _commandService = new();
    private readonly Mock<IBookingReferenceDataService> _referenceDataService = new();
    private readonly Mock<IBookingValidationService> _validationService = new();
    private readonly Mock<IBookingExportService> _exportService = new();
    private readonly Mock<ILogger<BookingController>> _logger = new();

    private BookingController CreateController(
        Mock<UserManager<ApplicationUser>>? userManager = null,
        string? username = "user@test.com",
        string? role = null)
    {
        var mgr = userManager ?? ControllerTestHelpers.CreateMockUserManager();
        var controller = new BookingController(
            _queryService.Object,
            _commandService.Object,
            _referenceDataService.Object,
            _validationService.Object,
            _exportService.Object,
            mgr.Object,
            _logger.Object);
        controller.ControllerContext = ControllerTestHelpers.CreateControllerContext(username, role: role);
        controller.TempData = new TempDataDictionary(controller.HttpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    private void SetupReferenceData()
    {
        _referenceDataService.Setup(s => s.AgeLimits).Returns(["General Audience", "Adults Only"]);
        _referenceDataService.Setup(s => s.Channels).Returns(["BBC One", "HBO"]);
        _referenceDataService.Setup(s => s.Genres).Returns(["Action", "Comedy"]);
    }

    [Fact]
    public async Task Index_WhenUnauthenticated_ReturnsViewWithAllBookings()
    {
        var bookings = new List<Booking> { new() { Id = 1, Program = "Test", RoomNumber = 2 } };
        _queryService.Setup(s => s.GetFilteredBookingsAsync(null, null)).ReturnsAsync(bookings);
        _queryService.Setup(s => s.GetNearBookingsAsync()).ReturnsAsync([]);

        var controller = CreateController(username: null);

        var result = await controller.Index(null, false);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingIndexViewModel>(viewResult.Model);
        Assert.Single(vm.Bookings);
        Assert.False(vm.MyBookings);
    }

    [Fact]
    public async Task Index_WithAgeFilter_FiltersByAgeLimit()
    {
        _queryService.Setup(s => s.GetFilteredBookingsAsync(new[] { "Adults Only" }, null)).ReturnsAsync([]);
        _queryService.Setup(s => s.GetNearBookingsAsync()).ReturnsAsync([]);

        var controller = CreateController(username: null);

        var result = await controller.Index(["Adults Only"], false);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingIndexViewModel>(viewResult.Model);
        Assert.Equal(["Adults Only"], vm.SelectedAgeLimits);
    }

    [Fact]
    public async Task Index_WithMyBookings_FiltersByUserRoom()
    {
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerGetUser(userManager, user);
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);

        _queryService.Setup(s => s.GetFilteredBookingsAsync(null, 5)).ReturnsAsync([]);
        _queryService.Setup(s => s.GetNearBookingsAsync()).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Index(null, true);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingIndexViewModel>(viewResult.Model);
        Assert.Equal(5, vm.CurrentUserRoomNumber);
    }

    [Fact]
    public async Task Details_WhenBookingExists_ReturnsViewWithBooking()
    {
        var booking = new Booking { Id = 1, Program = "Test" };
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(username: null);

        var result = await controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingDeleteViewModel>(viewResult.Model);
        Assert.Equal(1, vm.Booking.Id);
    }

    [Fact]
    public async Task Details_WhenIdIsNull_ReturnsNotFound()
    {
        var controller = CreateController(username: null);
        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WhenBookingNotFound_ReturnsNotFound()
    {
        _queryService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        var controller = CreateController(username: null);

        var result = await controller.Details(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateGet_WhenGuest_SetsRoomFromCurrentUser()
    {
        SetupReferenceData();
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingFormViewModel>(viewResult.Model);
        Assert.Equal(5, vm.Booking.RoomNumber);
    }

    [Fact]
    public async Task CreateGet_WhenAdmin_SetsDefaultRoom()
    {
        SetupReferenceData();
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);

        var controller = CreateController(username: "admin@test.com", role: RoleNames.Admin);

        var result = await controller.Create();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingFormViewModel>(viewResult.Model);
        Assert.Equal(BookingConstants.AdminRoomNumber, vm.Booking.RoomNumber);
    }

    [Fact]
    public async Task CreatePost_WithValidBooking_CreatesAndRedirects()
    {
        SetupReferenceData();
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);
        _validationService.Setup(s => s.ValidateAsync(It.IsAny<Booking>(), null, null)).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");
        var vm = new BookingFormViewModel { Booking = new Booking { Program = "Test", RoomNumber = 5 } };

        var result = await controller.Create(vm);

        _commandService.Verify(s => s.CreateAsync(It.IsAny<Booking>()), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task CreatePost_WhenModelInvalid_ReturnsView()
    {
        SetupReferenceData();
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");
        controller.ModelState.AddModelError("Booking.Program", "Required");
        var vm = new BookingFormViewModel { Booking = new Booking() };

        var result = await controller.Create(vm);

        _commandService.Verify(s => s.CreateAsync(It.IsAny<Booking>()), Times.Never);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreatePost_WithValidationErrors_ReturnsView()
    {
        SetupReferenceData();
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);
        _validationService.Setup(s => s.ValidateAsync(It.IsAny<Booking>(), null, null))
            .ReturnsAsync([new BookingValidationError("RoomNumber", "No such room")]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");
        var vm = new BookingFormViewModel { Booking = new Booking { Program = "Test", RoomNumber = 999 } };

        var result = await controller.Create(vm);

        _commandService.Verify(s => s.CreateAsync(It.IsAny<Booking>()), Times.Never);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task EditGet_WhenOwnBooking_ReturnsView()
    {
        SetupReferenceData();
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 5 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingFormViewModel>(viewResult.Model);
        Assert.Equal(1, vm.Booking.Id);
    }

    [Fact]
    public async Task EditGet_WhenWrongRoom_ReturnsForbid()
    {
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 10 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Edit(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task EditGet_WhenAdmin_ReturnsViewForAnyBooking()
    {
        SetupReferenceData();
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 5 };
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);

        var controller = CreateController(username: "admin@test.com", role: RoleNames.Admin);

        var result = await controller.Edit(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingFormViewModel>(viewResult.Model);
        Assert.Equal(1, vm.Booking.Id);
    }

    [Fact]
    public async Task EditGet_WhenBookingNotFound_ReturnsNotFound()
    {
        _queryService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        var controller = CreateController(username: null);

        var result = await controller.Edit(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditPost_ValidUpdate_UpdatesAndRedirects()
    {
        SetupReferenceData();
        var booking = new Booking { Id = 1, Program = "Old", RoomNumber = 5 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);
        _queryService.Setup(s => s.GetFreeTimeSlotsAsync()).ReturnsAsync([]);
        _validationService.Setup(s => s.ValidateAsync(It.IsAny<Booking>(), 1, null)).ReturnsAsync([]);

        var controller = CreateController(userManager: userManager, username: "user@test.com");
        var vm = new BookingFormViewModel { Booking = new Booking { Id = 1, Program = "Updated", RoomNumber = 5 } };

        var result = await controller.Edit(1, vm);

        _commandService.Verify(s => s.UpdateAsync(It.IsAny<Booking>()), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task EditPost_WhenIdMismatch_ReturnsNotFound()
    {
        var controller = CreateController(username: "user@test.com");
        var vm = new BookingFormViewModel { Booking = new Booking { Id = 2 } };

        var result = await controller.Edit(1, vm);

        _commandService.Verify(s => s.UpdateAsync(It.IsAny<Booking>()), Times.Never);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteGet_WhenOwnBooking_ReturnsView()
    {
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 5 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<BookingDeleteViewModel>(viewResult.Model);
        Assert.Equal(1, vm.Booking.Id);
    }

    [Fact]
    public async Task DeleteGet_WhenAdmin_ReturnsViewForAnyBooking()
    {
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 5 };
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(username: "admin@test.com", role: RoleNames.Admin);

        var result = await controller.Delete(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<BookingDeleteViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task DeleteGet_WhenWrongRoom_ReturnsForbid()
    {
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 10 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.Delete(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Valid_DeletesAndRedirects()
    {
        var booking = new Booking { Id = 1, Program = "Test", RoomNumber = 5 };
        var user = new ApplicationUser { Id = "1", RoomNumber = 5 };
        var userManager = ControllerTestHelpers.CreateMockUserManager();
        ControllerTestHelpers.SetupUserManagerFindByName(userManager, user);
        _queryService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(booking);

        var controller = CreateController(userManager: userManager, username: "user@test.com");

        var result = await controller.DeleteConfirmed(1);

        _commandService.Verify(s => s.DeleteAsync(booking), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task DeleteConfirmed_WhenNotFound_ReturnsNotFound()
    {
        _queryService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Booking?)null);

        var controller = CreateController(username: null);

        var result = await controller.DeleteConfirmed(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Statistics_ReturnsViewWithStatistics()
    {
        var stats = new StatisticsViewModel { TotalBookings = 10 };
        _queryService.Setup(s => s.GetStatisticsAsync(null, null)).ReturnsAsync(stats);

        var controller = CreateController(username: null);

        var result = await controller.Statistics(null, null);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StatisticsViewModel>(viewResult.Model);
        Assert.Equal(10, model.TotalBookings);
    }

    [Fact]
    public async Task XmlExportPost_ReturnsFileResult()
    {
        var date = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _exportService.Setup(s => s.ExportBookingsToXmlAsync(date)).ReturnsAsync([1, 2, 3]);

        var controller = CreateController(username: null);

        var result = await controller.XmlExportPost(date);

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/xml", fileResult.ContentType);
        Assert.Equal("bookings_2025-06-01.xml", fileResult.FileDownloadName);
    }
}
