# RaceDay API

A RESTful Web API for managing running, walking and cycling events, enrolments and results. This is Part 2 of the RaceDay system.

## Technologies

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core (Code-First) with SQL Server
- Session-based authentication with BCrypt password hashing
- Swagger UI for viewing and testing endpoints
- xUnit tests
- GitHub Actions for CI

## Roles

- **Organiser**: creates and manages their own events, categories, routes, weather, and captures results.
- **Participant**: enrols in events, cancels their own enrolment, and views their own results.

## How to run

1. Open the solution in Visual Studio.
2. In `appsettings.json`, set the `DefaultConnection` string to your SQL Server.
3. In the Package Manager Console, run `Update-Database` to create the database.
4. Run the `RaceDayApi` project and open `https://localhost:7130/swagger`.
5. Register a user, log in, and test the endpoints. Login creates a session, so log out before switching users.

## How to run the tests

- In Visual Studio: Test > Test Explorer > Run All Tests.
- Or in a terminal: `dotnet test`

The tests use an in-memory database, so they do not touch the real database. They cover registration and login, event management, role-based access and event enrolments (success and failure cases).

## Endpoints

| Area | Method | Route | Access |
|------|--------|-------|--------|
| Auth | POST | /api/auth/register | Public |
| Auth | POST | /api/auth/login | Public |
| Auth | POST | /api/auth/logout | Public |
| Profile | GET | /api/profile | Any logged-in user |
| Profile | PUT | /api/profile | Any logged-in user |
| Events | GET | /api/events | Public |
| Events | GET | /api/events/{id} | Public |
| Events | POST | /api/events | Organiser |
| Events | PUT | /api/events/{id} | Organiser (owner) |
| Events | DELETE | /api/events/{id} | Organiser (owner) |
| Categories | GET | /api/events/{eventId}/categories | Public |
| Categories | POST | /api/events/{eventId}/categories | Organiser (owner) |
| Categories | PUT | /api/categories/{id} | Organiser (owner) |
| Categories | DELETE | /api/categories/{id} | Organiser (owner) |
| Enrolments | POST | /api/events/{eventId}/enrolments | Participant |
| Enrolments | GET | /api/events/{eventId}/enrolments | Organiser (owner) |
| Enrolments | DELETE | /api/enrolments/{id} | Participant (owner) |
| Results | POST | /api/events/{eventId}/results | Organiser (owner) |
| Results | GET | /api/events/{eventId}/results | Public |
| Results | GET | /api/users/me/results | Participant |
| Routes | GET | /api/events/{eventId}/route | Public |
| Routes | POST | /api/events/{eventId}/route | Organiser (owner) |
| Weather | GET | /api/events/{eventId}/weather | Public |
| Weather | POST | /api/events/{eventId}/weather | Organiser (owner) |

## Notes

- Passwords are hashed with BCrypt and never stored in their original form.
- Non-public endpoints return 401 when not logged in and 403 when the role or ownership is wrong.
- Event types are Running, Walking and Cycling.
- The database schema follows the Part 1 SQL script.

## Continuous integration

Every push to `main` runs the GitHub Actions workflow (`.github/workflows/dotnet.yml`), which restores, builds and runs all tests.
