using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVBookingMVC.Models;

namespace TVBookingMVC.Areas.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());

        int bookingId = 10;
        for (int i = 2; i < 22; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                int bookings = new Random().Next(0, 5);
                for (int k = 0; k < bookings; k++)
                {
                    var start = DateTime.Now.AddDays(-j).AddHours(new Random().Next(24 / bookings) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {new Random().Next(1, 100)}",
                        Channel = Globals.Channels[new Random().Next(0, Globals.Channels.Length)],
                        Genre = Globals.Genres[new Random().Next(0, Globals.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(new Random().Next(30, 121)),
                        AgeLimit = Globals.AgeLimits[new Random().Next(0, Globals.AgeLimits.Length)],
                        RoomNumber = i,
                    });
                }
            }

            for (int j = 0; j < 7; j++)
            {
                int bookings = new Random().Next(0, 3);
                for (int k = 0; k < bookings; k++)
                {
                    var start = DateTime.Now.AddDays(j).AddHours(new Random().Next(24 / bookings) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {new Random().Next(1, 100)}",
                        Channel = Globals.Channels[new Random().Next(0, Globals.Channels.Length)],
                        Genre = Globals.Genres[new Random().Next(0, Globals.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(new Random().Next(30, 121)),
                        AgeLimit = Globals.AgeLimits[new Random().Next(0, Globals.AgeLimits.Length)],
                        RoomNumber = i,
                    });
                }
            }
        }

    }
}

public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.RoomNumber).IsRequired();
        builder.HasIndex(u => u.RoomNumber).IsUnique();
        builder.Property(u => u.RoomNumber).HasAnnotation("Range", new[] { 1, 999 });
    }
}