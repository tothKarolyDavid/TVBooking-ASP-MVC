using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;
using TVBookingMVC.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

var useSqlite = string.Equals(Environment.GetEnvironmentVariable("TVBOOKING_USE_SQLITE"), "true", StringComparison.OrdinalIgnoreCase);

if (useSqlite)
{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    builder.Services.AddSingleton(connection);
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
}

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();

var app = builder.Build();

if (useSqlite)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Booking}/{action=Index}/{id?}");

app.MapRazorPages();

await SeedIdentityAsync(app);

app.Run();

static async Task SeedIdentityAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
    }

    await EnsureUserAsync(userManager, "admin@hotel.com", 999, assignAdmin: true);
    await EnsureUserAsync(userManager, "room2@hotel.com", 2, assignAdmin: false);
}

static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, int roomNumber, bool assignAdmin)
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

        var result = await userManager.CreateAsync(user, "Password1!");
        if (!result.Succeeded)
        {
            return;
        }
    }
    else if (user.RoomNumber != roomNumber)
    {
        user.RoomNumber = roomNumber;
        await userManager.UpdateAsync(user);
    }

    if (assignAdmin && !await userManager.IsInRoleAsync(user, RoleNames.Admin))
    {
        await userManager.AddToRoleAsync(user, RoleNames.Admin);
    }
}

public partial class Program
{
}
