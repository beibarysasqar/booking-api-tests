using BookingTests.Models;

namespace BookingTests.Tests;

[TestFixture]
public class UpdateDeleteTests : ApiTestBase
{
    [Test]
    public async Task Put_ValidToken_ReplacesBooking()
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());
        var updated = new BookingBuilder().WithFirstName("Anna").WithTotalPrice(999).WithDepositPaid(false).Build();

        var response = await Api.UpdateBookingAsync(created.BookingId, updated, Token);
        var saved = await BookingApiClient.ReadAsync<Booking>(await Api.GetBookingAsync(created.BookingId));

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.Multiple(() =>
        {
            Assert.That(saved.FirstName, Is.EqualTo("Anna"));
            Assert.That(saved.TotalPrice, Is.EqualTo(999));
            Assert.That(saved.DepositPaid, Is.False);
        });
    }

    [Test]
    public async Task Put_WithoutToken_Returns403()
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.UpdateBookingAsync(created.BookingId, new BookingBuilder().Build(), null);

        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }

    [TestCase("")]
    [TestCase("wrong-token")]
    public async Task Put_InvalidToken_Returns403(string token)
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.UpdateBookingAsync(created.BookingId, new BookingBuilder().Build(), token);

        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Patch_ValidToken_ChangesOnlyGivenField()
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.PatchBookingAsync(created.BookingId, new { firstname = "Patched" }, Token);
        var saved = await BookingApiClient.ReadAsync<Booking>(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That(saved.FirstName, Is.EqualTo("Patched"));
        Assert.That(saved.LastName, Is.EqualTo(created.Booking.LastName), "Other fields must stay the same");
    }

    [TestCase(null)]
    [TestCase("wrong-token")]
    public async Task Patch_NoOrWrongToken_Returns403(string? token)
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.PatchBookingAsync(created.BookingId, new { firstname = "Hacker" }, token);

        Assert.That((int)response.StatusCode, Is.EqualTo(403));
    }

    [Test]
    public async Task Delete_ValidToken_BookingIsGone()
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.DeleteBookingAsync(created.BookingId, Token);
        var getAfter = await Api.GetBookingAsync(created.BookingId);

        // 201 for a delete looks odd, but that's what the docs say
        Assert.That((int)response.StatusCode, Is.EqualTo(201));
        Assert.That((int)getAfter.StatusCode, Is.EqualTo(404));
    }

    [TestCase(null)]
    [TestCase("wrong-token")]
    public async Task Delete_NoOrWrongToken_Returns403AndBookingStays(string? token)
    {
        var created = await CreateBookingAsync(new BookingBuilder().Build());

        var response = await Api.DeleteBookingAsync(created.BookingId, token);
        var getAfter = await Api.GetBookingAsync(created.BookingId);

        Assert.That((int)response.StatusCode, Is.EqualTo(403));
        Assert.That((int)getAfter.StatusCode, Is.EqualTo(200));
    }

    // DEF-007: returns 405 instead of 404
    [Test]
    [Category("KnownBug")]
    public async Task Delete_NotExistingBooking_Returns404()
    {
        var response = await Api.DeleteBookingAsync(999999999, Token);

        Assert.That((int)response.StatusCode, Is.EqualTo(404), "DEF-007");
    }
}
