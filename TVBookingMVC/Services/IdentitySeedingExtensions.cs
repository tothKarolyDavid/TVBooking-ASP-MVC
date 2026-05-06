using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Constants;

namespace TVBookingMVC.Services;

public static class IdentitySeedingExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(RoleNames.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
        }

        await EnsureUserAsync(userManager, "admin@hotel.com", 0, assignAdmin: true);
        await EnsureUserAsync(userManager, "room2@hotel.com", 2, assignAdmin: false);
        for (int room = 3; room <= 21; room++)
        {
            await EnsureUserAsync(userManager, $"room{room}@hotel.com", room, assignAdmin: false);
        }
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, int roomNumber, bool assignAdmin)
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

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return;
            }
        }
        else if (user.RoomNumber != roomNumber)
        {
            var existingRoomUser = await userManager.Users
                .FirstOrDefaultAsync(u => u.RoomNumber == roomNumber && u.Id != user.Id);
            if (existingRoomUser != null)
                return;

            user.RoomNumber = roomNumber;
            await userManager.UpdateAsync(user);
        }

        if (assignAdmin && !await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            await userManager.AddToRoleAsync(user, RoleNames.Admin);
        }
    }
}