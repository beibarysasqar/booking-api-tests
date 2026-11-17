using System.Text.Json.Serialization;

namespace BookingTests.Models;

public class Booking
{
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("totalprice")]
    public int TotalPrice { get; set; }

    [JsonPropertyName("depositpaid")]
    public bool DepositPaid { get; set; }

    [JsonPropertyName("bookingdates")]
    public BookingDates BookingDates { get; set; } = new();

    [JsonPropertyName("additionalneeds")]
    public string? AdditionalNeeds { get; set; }
}

// strings, not DateTime - otherwise there's no way to send a date like "2026-13-45"
public class BookingDates
{
    [JsonPropertyName("checkin")]
    public string CheckIn { get; set; } = "";

    [JsonPropertyName("checkout")]
    public string CheckOut { get; set; } = "";
}

// what POST /booking returns
public class CreatedBooking
{
    [JsonPropertyName("bookingid")]
    public int BookingId { get; set; }

    [JsonPropertyName("booking")]
    public Booking Booking { get; set; } = new();
}

// one item from GET /booking
public class BookingIdItem
{
    [JsonPropertyName("bookingid")]
    public int BookingId { get; set; }
}
