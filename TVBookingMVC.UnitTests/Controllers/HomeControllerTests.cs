using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TVBookingMVC.Controllers;
using TVBookingMVC.Models;

namespace TVBookingMVC.UnitTests.Controllers;

public sealed class HomeControllerTests
{
    private static HomeController CreateController()
    {
        var logger = new Mock<ILogger<HomeController>>().Object;
        var controller = new HomeController(logger);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    [Fact]
    public void Index_ReturnsViewResult()
    {
        var controller = CreateController();
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsViewResult()
    {
        var controller = CreateController();
        var result = controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_ReturnsViewResult_WithErrorViewModel()
    {
        var controller = CreateController();
        var result = controller.Error();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ErrorViewModel>(viewResult.Model);
    }
}
