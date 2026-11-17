using BookingTests.Models;

namespace BookingTests;

public class BookingBuilder
{
    private string _firstName = "John";
    private string _lastName = "Smith";
    private int _totalPrice = 150;
    private bool _depositPaid = true;
    private string _checkIn = DateTime.Today.AddDays(10).ToString("yyyy-MM-dd");
    private string _checkOut = DateTime.Today.AddDays(13).ToString("yyyy-MM-dd");
    private string? _additionalNeeds = "Breakfast";

    public BookingBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public BookingBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public BookingBuilder WithTotalPrice(int totalPrice)
    {
        _totalPrice = totalPrice;
        return this;
    }

    public BookingBuilder WithDepositPaid(bool depositPaid)
    {
        _depositPaid = depositPaid;
        return this;
    }

    public BookingBuilder WithDates(string checkIn, string checkOut)
    {
        _checkIn = checkIn;
        _checkOut = checkOut;
        return this;
    }

    public Booking Build()
    {
        return new Booking
        {
            FirstName = _firstName,
            LastName = _lastName,
            TotalPrice = _totalPrice,
            DepositPaid = _depositPaid,
            BookingDates = new BookingDates { CheckIn = _checkIn, CheckOut = _checkOut },
            AdditionalNeeds = _additionalNeeds
        };
    }
}
