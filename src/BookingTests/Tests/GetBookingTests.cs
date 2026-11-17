using BookingTests.Models;

namespace BookingTests.Tests;

[TestFixture]
public class GetBookingTests : ApiTestBase
{
    [Test]
    public async Task GetById_ExistingBooking_ReturnsIt()
    {
        var created = await CreateBookingAsync(new BookingBuilder().WithFirstName(UniqueName()).Build());

        var response = await Api.GetBookingAsync(created.BookingId);
        var booking = await BookingApiClient.ReadAsync<Booking>(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(booking.FirstName, Is.EqualTo(created.Booking.FirstName));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(999999999)]
    public async Task GetById_NotExisting_Returns404(int id)
    {
        var response = await Api.GetBookingAsync(id);

        Assert.That((int)response.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetIds_NoFilter_ContainsNewBookingAndNoDuplicates()
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var ids = await GetIds(null);

        Assert.That(ids, Does.Contain(created.BookingId));
        Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "List has duplicate ids");
    }

    [Test]
    public async Task GetIds_FilterByName_ReturnsOnlyMatchingBookings()
    {
        var name = UniqueName();
        var first = await CreateBookingAsync(new BookingBuilder().WithFirstName(name).Build());
        var second = await CreateBookingAsync(new BookingBuilder().WithFirstName(name).Build());
        await CreateBookingAsync(new BookingBuilder().WithFirstName(UniqueName()).Build());

        var ids = await GetIds($"firstname={name}");

        Assert.That(ids, Is.EquivalentTo(new[] { first.BookingId, second.BookingId }));
    }

    [Test]
    public async Task GetIds_UnknownName_ReturnsEmptyList()
    {
        var ids = await GetIds($"firstname={UniqueName()}");

        Assert.That(ids, Is.Empty);
    }

    // docs say the checkin filter means checkin >= date
    // DEF-006: it actually works as >, so a booking on that exact day isn't found
    [Test]
    [Category("KnownBug")]
    public async Task GetIds_FilterByCheckin_IncludesBookingOnThatDate()
    {
        var name = UniqueName();
        var created = await CreateBookingAsync(new BookingBuilder()
            .WithFirstName(name)
            .WithDates(Date(20), Date(22))
            .Build());

        var ids = await GetIds($"firstname={name}&checkin={Date(20)}");

        Assert.That(ids, Does.Contain(created.BookingId), "DEF-006");
    }

    private async Task<List<int>> GetIds(string? query)
    {
        var response = await Api.GetBookingIdsAsync(query);
        Assert.That((int)response.StatusCode, Is.EqualTo(200));

        var items = await BookingApiClient.ReadAsync<List<BookingIdItem>>(response);
        return items.Select(i => i.BookingId).ToList();
    }
}
