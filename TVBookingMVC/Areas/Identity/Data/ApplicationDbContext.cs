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

        var rng = new Random(41205);
        var baseDate = DateTime.Today.AddHours(8);
        int bookingId = 10;
        for (int i = 2; i < 22; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                int bookings = rng.Next(0, 5);
                for (int k = 0; k < bookings; k++)
                {
                    var hoursPerSlot = Math.Max(1, 24 / bookings);
                    var start = baseDate.AddDays(-j).AddHours(rng.Next(hoursPerSlot) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {rng.Next(1, 100)}",
                        Channel = BookingReferenceData.Channels[rng.Next(0, BookingReferenceData.Channels.Length)],
                        Genre = BookingReferenceData.Genres[rng.Next(0, BookingReferenceData.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(rng.Next(30, 121)),
                        AgeLimit = BookingReferenceData.AgeLimits[rng.Next(0, BookingReferenceData.AgeLimits.Length)],
                        RoomNumber = i,
                    });
                }
            }

            for (int j = 0; j < 7; j++)
            {
                int bookings = rng.Next(0, 3);
                for (int k = 0; k < bookings; k++)
                {
                    var hoursPerSlot = Math.Max(1, 24 / bookings);
                    var start = baseDate.AddDays(j).AddHours(rng.Next(hoursPerSlot) * k);

                    builder.Entity<Booking>().HasData(new Booking
                    {
                        Id = bookingId++,
                        Program = $"Program {rng.Next(1, 100)}",
                        Channel = BookingReferenceData.Channels[rng.Next(0, BookingReferenceData.Channels.Length)],
                        Genre = BookingReferenceData.Genres[rng.Next(0, BookingReferenceData.Genres.Length)],
                        Start = start,
                        End = start.AddMinutes(rng.Next(30, 121)),
                        AgeLimit = BookingReferenceData.AgeLimits[rng.Next(0, BookingReferenceData.AgeLimits.Length)],
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
        builder.Property(u => u.RoomNumber).HasAnnotation("Range", new[] { 0, 999 });
    }
}