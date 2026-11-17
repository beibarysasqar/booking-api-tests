using BookingTests.Models;

namespace BookingTests.Tests;

public abstract class ApiTestBase
{
    protected BookingApiClient Api = null!;
    protected string Token = null!;

    // bookings made during a test, deleted in TearDown - the server is shared, no need to leave junk there
    private readonly List<int> _createdIds = new();

    [OneTimeSetUp]
    public async Task BaseOneTimeSetUp()
    {
        Api = new BookingApiClient();
        Token = await Api.GetTokenAsync() ?? throw new InvalidOperationException("Could not get auth token");
    }

    [OneTimeTearDown]
    public void BaseOneTimeTearDown()
    {
        Api.Dispose();
    }

    [TearDown]
    public async Task DeleteCreatedBookings()
    {
        foreach (var id in _createdIds)
            await Api.DeleteBookingAsync(id, Token);
        _createdIds.Clear();
    }

    protected async Task<CreatedBooking> CreateBookingAsync(Booking booking)
    {
        var response = await Api.CreateBookingAsync(booking);
        Assert.That((int)response.StatusCode, Is.EqualTo(200), "Precondition: booking was not created");

        var created = await BookingApiClient.ReadAsync<CreatedBooking>(response);
        _createdIds.Add(created.BookingId);
        return created;
    }

    // negative tests: if the API wrongly accepted the booking, delete it anyway
    protected async Task RememberIfCreated(HttpResponseMessage response)
    {
        if ((int)response.StatusCode != 200)
            return;
        var created = await BookingApiClient.ReadAsync<CreatedBooking>(response);
        _createdIds.Add(created.BookingId);
    }

    protected static string Date(int daysFromToday)
    {
        return DateTime.Today.AddDays(daysFromToday).ToString("yyyy-MM-dd");
    }

    protected static string UniqueName()
    {
        return "Test" + Guid.NewGuid().ToString("N")[..8];
    }
}
