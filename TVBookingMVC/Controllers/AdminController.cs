using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TVBookingMVC.Constants;
using TVBookingMVC.Services;

namespace TVBookingMVC.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class AdminController : Controller
{
    private readonly IDatabaseReseedService _reseedService;

    public AdminController(IDatabaseReseedService reseedService)
    {
        _reseedService = reseedService;
    }

    [HttpGet]
    public IActionResult Reseed()
    {
        return View();
    }

    [HttpPost]
    [ActionName("Reseed")]
    public async Task<IActionResult> ReseedConfirmed()
    {
        await _reseedService.ReseedAsync();
        TempData["Success"] = "Database reseeded successfully.";
        return RedirectToAction("Index", "Booking");
    }
}
