using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TVBookingMVC.Areas.Identity.Data;
using TVBookingMVC.Services;

var app = Program.CreateApp(args);
await app.SeedIdentityAsync();
app.Run();

public partial class Program
{
    public static WebApplication CreateApp(string[]? args = null, Action<IServiceCollection>? configureServices = null, string? contentRoot = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args ?? [],
            ContentRootPath = contentRoot ?? Directory.GetCurrentDirectory(),
            ApplicationName = typeof(Program).Assembly.GetName().Name
        });
        var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

        builder.Services
            .AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IDatabaseReseedService, DatabaseReseedService>();
        builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();
        builder.Services.AddScoped<IBookingReferenceDataService, BookingReferenceDataService>();
        builder.Services.AddScoped<IBookingQueryService, BookingQueryService>();
        builder.Services.AddScoped<IBookingCommandService, BookingCommandService>();
        builder.Services.AddScoped<IBookingExportService, BookingExportService>();

        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        var cultureInfo = new CultureInfo("en-US");
        cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
        cultureInfo.DateTimeFormat.LongDatePattern = "yyyy/MM/dd";
        cultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm";
        cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";

        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(cultureInfo),
            SupportedCultures = [cultureInfo],
            SupportedUICultures = [cultureInfo]
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Booking}/{action=Index}/{id?}");

        app.MapRazorPages();

        return app;
    }
}
