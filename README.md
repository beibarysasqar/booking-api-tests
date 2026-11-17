# Booking API Tests

My API tests for [Restful-booker](https://restful-booker.herokuapp.com), a public demo API for hotel bookings.
Written in C# with NUnit and plain `HttpClient`.

I test login, creating, reading, updating and deleting bookings. Test data is picked by equivalence
classes and boundary values: for example price 0 and 1 should be accepted and -1 rejected, a one-night
stay is fine but checkout on the checkin day is not, and update/delete are tried with no token, a wrong
token and a valid one.

Right now there are 41 test cases. 30 pass, the other 11 fail because of bugs in the API itself,
which I wrote up in [docs/defects.md](docs/defects.md).

## What's where

- `src/BookingTests/` - small API client, a builder for test bookings, and the tests (`Tests/`)
- `scripts/run-tests.ps1` - runs the tests, reads the TRX report and prints a short summary
- `docs/defects.md` - the bugs I found
- `.gitlab-ci.yml`, `azure-pipelines.yml` - CI setup

## How to run

You need the .NET 10 SDK. PowerShell 7 only if you want to use the script.

```
dotnet test
dotnet test --filter "TestCategory!=KnownBug"
pwsh ./scripts/run-tests.ps1
pwsh ./scripts/run-tests.ps1 -Filter ""
```

The script skips known bugs unless you pass `-Filter ""`, and returns exit code 1 if anything fails
or if no tests ran at all (usually a typo in the filter).

The API address and login can be changed with environment variables. If they're not set, the public
demo values from the Restful-booker docs are used:

| Variable | Default |
|---|---|
| `BOOKING_BASE_URL` | `https://restful-booker.herokuapp.com` |
| `BOOKING_USER` | `admin` |
| `BOOKING_PASSWORD` | `password123` |

Every test creates the bookings it needs and deletes them at the end, so nothing depends on data
that someone else put on the shared server.

## Known bugs

If a test fails because the API is wrong, I mark it `[Category("KnownBug")]` and put the defect
number (DEF-001 to DEF-007) next to it. CI leaves these out, so a red pipeline means something new
broke, not the same old bugs again. When a bug gets fixed, the test will start passing and the
category can be removed.

## CI

- GitLab: `test-linux` runs on every push using the `dotnet/sdk:10.0` image. `test-windows` runs on
  GitLab's shared Windows runner, but only when started by hand. The TRX file is kept as an artifact.
- Azure Pipelines: runs the same script on Ubuntu and Windows and publishes the results to the
  Tests tab.
