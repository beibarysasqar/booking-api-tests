namespace BookingTests.Tests;

[TestFixture]
public class AuthTests
{
    private BookingApiClient _api = null!;

    [OneTimeSetUp]
    public void SetUp() => _api = new BookingApiClient();

    [OneTimeTearDown]
    public void TearDown() => _api.Dispose();

    [Test]
    public async Task GetToken_ValidCredentials_ReturnsToken()
    {
        var token = await _api.GetTokenAsync();

        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }

    [TestCase("admin", "wrong-password")]
    [TestCase("not-admin", "password123")]
    [TestCase("ADMIN", "password123")]
    [TestCase("", "")]
    public async Task GetToken_InvalidCredentials_NoToken(string username, string password)
    {
        var token = await _api.GetTokenAsync(username, password);

        Assert.That(token, Is.Null);
    }
}
