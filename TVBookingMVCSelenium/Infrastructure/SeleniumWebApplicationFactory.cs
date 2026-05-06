using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TVBookingMVC;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;

namespace TVBookingMVCSelenium.Infrastructure;

public class SeleniumWebApplicationFactory : IAsyncLifetime
{
    private WebApplication? _app;
    private SqliteConnection? _connection;

    public string BaseUrl { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var mvcProjectDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "TVBookingMVC"));

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        _app = Program.CreateApp(
            args: null,
            configureServices: services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(_connection));
            },
            contentRoot: mvcProjectDir);

        _app.Urls.Add("http://127.0.0.1:0");
        await _app.StartAsync();

        BaseUrl = _app.Urls.First();

        using var scope = _app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));

        await EnsureUserAsync(userManager, "admin@hotel.com", 0, assignAdmin: true);
        await EnsureUserAsync(userManager, "room2@hotel.com", 2);
        for (int room = 3; room <= 21; room++)
            await EnsureUserAsync(userManager, $"room{room}@hotel.com", room);
    }

    public async Task DisposeAsync()
    {
        if (_app != null)
            await _app.StopAsync();

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        int roomNumber,
        bool assignAdmin = false)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                RoomNumber = roomNumber
            };
            await userManager.CreateAsync(user);
        }

        if (assignAdmin && !await userManager.IsInRoleAsync(user, RoleNames.Admin))
            await userManager.AddToRoleAsync(user, RoleNames.Admin);
    }
}
