using System.Net;
using RSSFeedReader.Api.Services;
using Xunit;

namespace RSSFeedReader.Api.Tests;

public class FeedValidationServiceTests
{
    private const string ValidFeedXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Feed</title>
            <link>https://example.com/</link>
            <description>A test feed</description>
          </channel>
        </rss>
        """;

    private const string NotAFeedHtml = "<html><body>Not a feed</body></html>";

    private static FeedValidationService CreateService(HttpStatusCode statusCode, string content, string mediaType = "application/rss+xml")
    {
        var handler = new FakeHttpMessageHandler(statusCode, content, mediaType);
        var httpClient = new HttpClient(handler);
        var factory = new FakeHttpClientFactory(httpClient);
        return new FeedValidationService(factory);
    }

    [Theory]
    [InlineData("xxx")]
    [InlineData("not a url")]
    [InlineData("ftp://example.com/feed.xml")]
    public async Task ValidateAsync_WithMalformedOrUnsupportedSchemeUrl_ReturnsInvalid(string url)
    {
        var service = CreateService(HttpStatusCode.OK, ValidFeedXml);

        var result = await service.ValidateAsync(url);

        Assert.False(result.IsValid);
        Assert.Contains("valid http/https", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_WithNonFeedResponse_ReturnsInvalid()
    {
        var service = CreateService(HttpStatusCode.OK, NotAFeedHtml, "text/html");

        var result = await service.ValidateAsync("https://example.com/");

        Assert.False(result.IsValid);
        Assert.Contains("could not verify a feed", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateAsync_WithValidFeedResponse_ReturnsValid()
    {
        var service = CreateService(HttpStatusCode.OK, ValidFeedXml);

        var result = await service.ValidateAsync("https://example.com/feed.xml");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    private sealed class FakeHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class FakeHttpMessageHandler(HttpStatusCode statusCode, string content, string mediaType) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
            return Task.FromResult(response);
        }
    }
}
