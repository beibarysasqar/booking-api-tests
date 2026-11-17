using System.Net.Http.Json;
using System.Text.Json;

namespace BookingTests;

public class BookingApiClient : IDisposable
{
    private readonly HttpClient _http;

    public BookingApiClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("BOOKING_BASE_URL")
                      ?? "https://restful-booker.herokuapp.com";

        _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        // without it POST/PUT come back as 418 "I'm a Teapot" (yes, really)
        _http.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // a wrong login still gets 200, just with a "reason" field instead of a token - so return null
    public async Task<string?> GetTokenAsync(string? username = null, string? password = null)
    {
        username ??= Environment.GetEnvironmentVariable("BOOKING_USER") ?? "admin";
        password ??= Environment.GetEnvironmentVariable("BOOKING_PASSWORD") ?? "password123";

        var response = await _http.PostAsJsonAsync("/auth", new { username, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.TryGetProperty("token", out var token) ? token.GetString() : null;
    }

    public Task<HttpResponseMessage> GetBookingIdsAsync(string? query = null)
    {
        var url = string.IsNullOrEmpty(query) ? "/booking" : "/booking?" + query;
        return _http.GetAsync(url);
    }

    public Task<HttpResponseMessage> GetBookingAsync(int id)
    {
        return _http.GetAsync($"/booking/{id}");
    }

    // object instead of Booking: negative tests need to send broken bodies, e.g. without lastname
    public Task<HttpResponseMessage> CreateBookingAsync(object body)
    {
        return _http.PostAsJsonAsync("/booking", body);
    }

    public Task<HttpResponseMessage> UpdateBookingAsync(int id, object body, string? token)
    {
        return SendWithTokenAsync(HttpMethod.Put, $"/booking/{id}", body, token);
    }

    public Task<HttpResponseMessage> PatchBookingAsync(int id, object body, string? token)
    {
        return SendWithTokenAsync(HttpMethod.Patch, $"/booking/{id}", body, token);
    }

    public Task<HttpResponseMessage> DeleteBookingAsync(int id, string? token)
    {
        return SendWithTokenAsync(HttpMethod.Delete, $"/booking/{id}", null, token);
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new InvalidOperationException("Response body is empty");
    }

    // this API wants the token as a cookie, not as "Authorization: Bearer"
    private Task<HttpResponseMessage> SendWithTokenAsync(HttpMethod method, string url, object? body, string? token)
    {
        var request = new HttpRequestMessage(method, url);
        if (body != null)
            request.Content = JsonContent.Create(body);
        if (token != null)
            request.Headers.Add("Cookie", $"token={token}");

        return _http.SendAsync(request);
    }

    public void Dispose()
    {
        _http.Dispose();
    }
}
