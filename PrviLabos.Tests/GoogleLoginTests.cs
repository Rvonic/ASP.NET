using System.Net;
using Xunit;

namespace PrviLabos.Tests;

public sealed class GoogleLoginTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GoogleLoginTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task LoginPage_ShouldShowGoogleLogin_WhenGoogleIsConfigured()
    {
        var response = await _client.GetAsync("/racun/prijava");

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Google", html);
        Assert.Contains("name=\"provider\" value=\"Google\"", html);
    }

    [Fact]
    public async Task ExternalLogin_ShouldChallengeGoogleProvider()
    {
        var loginPage = await _client.GetAsync("/racun/prijava");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);

        var response = await _client.PostAsync(
            "/racun/vanjska-prijava",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["provider"] = "Google"
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("accounts.google.com", response.Headers.Location?.ToString());
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        const string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(start >= 0);

        start += marker.Length;
        var end = html.IndexOf('"', start);
        Assert.True(end > start);

        return html[start..end];
    }
}
