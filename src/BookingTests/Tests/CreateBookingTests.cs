using BookingTests.Models;

namespace BookingTests.Tests;

[TestFixture]
public class CreateBookingTests : ApiTestBase
{
    [Test]
    public async Task Create_ValidBooking_ReturnsIdAndSameData()
    {
        var booking = new BookingBuilder().WithFirstName(UniqueName()).Build();

        var created = await CreateBookingAsync(booking);

        Assert.Multiple(() =>
        {
            Assert.That(created.BookingId, Is.GreaterThan(0));
            Assert.That(created.Booking.FirstName, Is.EqualTo(booking.FirstName));
            Assert.That(created.Booking.LastName, Is.EqualTo(booking.LastName));
            Assert.That(created.Booking.TotalPrice, Is.EqualTo(booking.TotalPrice));
            Assert.That(created.Booking.BookingDates.CheckIn, Is.EqualTo(booking.BookingDates.CheckIn));
            Assert.That(created.Booking.BookingDates.CheckOut, Is.EqualTo(booking.BookingDates.CheckOut));
        });
    }

    // price >= 0 is valid. 0 and 1 = lower boundary, 1_000_000 to make sure there's no low upper limit
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(1_000_000)]
    public async Task Create_ValidTotalPrice_IsSaved(int price)
    {
        var created = await CreateBookingAsync(new BookingBuilder().WithTotalPrice(price).Build());

        Assert.That(created.Booking.TotalPrice, Is.EqualTo(price));
    }

    // DEF-001: negative price goes through
    [Category("KnownBug")]
    [TestCase(-1)]
    [TestCase(-500)]
    public async Task Create_NegativeTotalPrice_Returns400(int price)
    {
        var response = await Api.CreateBookingAsync(new BookingBuilder().WithTotalPrice(price).Build());
        await RememberIfCreated(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(400), "DEF-001");
    }

    // numbers are days from today. 1 night is the shortest valid stay, shorter ones are in the next test
    [TestCase(10, 11)]
    [TestCase(1, 30)]
    public async Task Create_ValidDates_AreSaved(int checkInDay, int checkOutDay)
    {
        var booking = new BookingBuilder().WithDates(Date(checkInDay), Date(checkOutDay)).Build();

        var created = await CreateBookingAsync(booking);

        Assert.That(created.Booking.BookingDates.CheckIn, Is.EqualTo(Date(checkInDay)));
        Assert.That(created.Booking.BookingDates.CheckOut, Is.EqualTo(Date(checkOutDay)));
    }

    // DEF-002: same-day or earlier checkout goes through
    [Category("KnownBug")]
    [TestCase(10, 10)]
    [TestCase(10, 9)]
    public async Task Create_CheckoutNotAfterCheckin_Returns400(int checkInDay, int checkOutDay)
    {
        var booking = new BookingBuilder().WithDates(Date(checkInDay), Date(checkOutDay)).Build();

        var response = await Api.CreateBookingAsync(booking);
        await RememberIfCreated(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(400), "DEF-002");
    }

    // DEF-003: invalid date is saved as "0NaN-aN-aN"
    [Category("KnownBug")]
    [TestCase("2026-13-45", "2026-12-10")]
    [TestCase("not-a-date", "2026-12-10")]
    public async Task Create_InvalidDateFormat_Returns400(string checkIn, string checkOut)
    {
        var response = await Api.CreateBookingAsync(new BookingBuilder().WithDates(checkIn, checkOut).Build());
        await RememberIfCreated(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(400), "DEF-003");
    }

    [TestCase(1)]
    [TestCase(255)]
    public async Task Create_NameLength_IsSavedAsIs(int length)
    {
        var name = new string('a', length);

        var created = await CreateBookingAsync(new BookingBuilder().WithFirstName(name).WithLastName(name).Build());

        Assert.That(created.Booking.FirstName, Is.EqualTo(name));
        Assert.That(created.Booking.LastName, Is.EqualTo(name));
    }

    // DEF-004: empty names go through
    [Category("KnownBug")]
    [TestCase("", "Smith")]
    [TestCase("John", "")]
    public async Task Create_EmptyName_Returns400(string firstName, string lastName)
    {
        var booking = new BookingBuilder().WithFirstName(firstName).WithLastName(lastName).Build();

        var response = await Api.CreateBookingAsync(booking);
        await RememberIfCreated(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(400), "DEF-004");
    }

    // DEF-005: 500 instead of 400 when lastname is missing
    [Test]
    [Category("KnownBug")]
    public async Task Create_WithoutLastName_Returns400()
    {
        var body = new
        {
            firstname = "John",
            totalprice = 100,
            depositpaid = true,
            bookingdates = new BookingDates { CheckIn = Date(5), CheckOut = Date(7) }
        };

        var response = await Api.CreateBookingAsync(body);
        await RememberIfCreated(response);

        Assert.That((int)response.StatusCode, Is.EqualTo(400), "DEF-005");
    }
}
