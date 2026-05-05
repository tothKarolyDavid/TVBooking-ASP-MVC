# TVBooking-ASP-MVC Agent Notes

Quick context for agents working in this repo. See [README.md](README.md) for product overview and screenshots.

## Build, run, test

```bash
dotnet build TVBooking.sln
dotnet run --project TVBookingMVC
dotnet test TVBookingMVCXUnit
dotnet test TVBookingMVCSelenium
```

App URLs come from [TVBookingMVC/Properties/launchSettings.json](TVBookingMVC/Properties/launchSettings.json). Default HTTPS is https://localhost:7233.

## Project layout

- MVC app: [TVBookingMVC/TVBookingMVC.csproj](TVBookingMVC/TVBookingMVC.csproj)
- xUnit tests: [TVBookingMVCXUnit/TVBookingMVCXUnit.csproj](TVBookingMVCXUnit/TVBookingMVCXUnit.csproj)
- Selenium tests: [TVBookingMVCSelenium/TVBookingMVCSelenium.csproj](TVBookingMVCSelenium/TVBookingMVCSelenium.csproj)

## Data and auth conventions

- EF Core DbContext: [TVBookingMVC/Areas/Identity/Data/ApplicationDbContext.cs](TVBookingMVC/Areas/Identity/Data/ApplicationDbContext.cs)
- Connection string in [TVBookingMVC/appsettings.json](TVBookingMVC/appsettings.json) uses local SQL Server with trusted connection.
- Admin checks use Identity roles (see [TVBookingMVC/Constants/RoleNames.cs](TVBookingMVC/Constants/RoleNames.cs)).

