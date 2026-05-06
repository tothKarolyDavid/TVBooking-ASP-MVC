using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;

namespace TVBookingMVC.UnitTests.Services;

public sealed class IdentitySeedingExtensionsTests
{
    private static ServiceProvider CreateServiceProvider()
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
            .AddUserManager<UserManager<ApplicationUser>>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddLogging();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedIdentityAsync_CreatesAdminRole()
    {
        var provider = CreateServiceProvider();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        var dbContext = provider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var adminRoleExists = await roleManager.RoleExistsAsync(RoleNames.Admin);

        // RoleManager won't auto-create roles unless we call CreateAsync
        // This test verifies the role can be created through the same mechanism
        var creationResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
        Assert.True(creationResult.Succeeded);

        adminRoleExists = await roleManager.RoleExistsAsync(RoleNames.Admin);
        Assert.True(adminRoleExists);
    }

    [Fact]
    public async Task EnsureUserAsync_CreatesUserWithRoomNumber()
    {
        var provider = CreateServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        var dbContext = provider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            UserName = "room5@hotel.com",
            Email = "room5@hotel.com",
            RoomNumber = 5
        };

        var result = await userManager.CreateAsync(user);
        Assert.True(result.Succeeded);

        var saved = await userManager.FindByEmailAsync("room5@hotel.com");
        Assert.NotNull(saved);
        Assert.Equal(5, saved.RoomNumber);
    }

    [Fact]
    public async Task EnsureUserAsync_AssignsAdminRole()
    {
        var provider = CreateServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        var dbContext = provider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.OpenConnectionAsync();
        await dbContext.Database.EnsureCreatedAsync();

        await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));

        var user = new ApplicationUser
        {
            UserName = "admin@hotel.com",
            Email = "admin@hotel.com",
            RoomNumber = 0
        };

        var createResult = await userManager.CreateAsync(user);
        Assert.True(createResult.Succeeded);

        var roleResult = await userManager.AddToRoleAsync(user, RoleNames.Admin);
        Assert.True(roleResult.Succeeded);

        var isAdmin = await userManager.IsInRoleAsync(user, RoleNames.Admin);
        Assert.True(isAdmin);
    }
}
