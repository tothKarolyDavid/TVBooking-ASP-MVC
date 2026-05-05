# TV Booking System

This is a simple TV booking system for a hotel that has only one TV in the common area.
The system has two interfaces, one for the admin and one for the guests. 
The admin can add new guests and bookings, and they can also view statistics about the bookings. 
The guests can add new bookings to their own room and they can also view their own bookings.

## Features

- Everyone:
  - View all bookings
  - View free time slots
  - View bookings by age rating

- Admin:
  - Add new guests
  - Add new bookings to any room
  - View statistics about the bookings
  - Export all the bookings to an XML from a given day

- Guests:
  - Login with their email and room number
  - Add new bookings to their own room        
  - View their own bookings
  - Get a notification 15 minutes before the start of a booking

## Prerequisites

- .NET 10 SDK
- SQL Server (localdb or full instance)

## Local setup

1. Update the connection string in [TVBookingMVC/appsettings.json](TVBookingMVC/appsettings.json) if needed.
2. Apply migrations and seed data:
  - `dotnet ef database update --project TVBookingMVC`

Seeded identities:
- Admin: `admin@hotel.com` (room 999)
- Guest: `room2@hotel.com` (room 2)

All seeded users use password `Password1!`.

## Run

`dotnet run --project TVBookingMVC`

Default HTTPS URL is listed in [TVBookingMVC/Properties/launchSettings.json](TVBookingMVC/Properties/launchSettings.json).

## Tests

- Unit tests: `dotnet test TVBookingMVCXUnit`
- Selenium tests: `dotnet test TVBookingMVCSelenium`

Selenium tests expect the app to already be running at the URL above and require Chrome/ChromeDriver.
	
## Screenshots

### Home page
![Home page](Docs/index.png)

### Statistics page
![Statistics page](Docs/statistics.png)
