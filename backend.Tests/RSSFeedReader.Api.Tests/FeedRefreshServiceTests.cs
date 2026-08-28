using System.Net;
using RSSFeedReader.Api.Services;
using Xunit;

namespace RSSFeedReader.Api.Tests;

public class FeedRefreshServiceTests
{
    private const string FeedWithItemsXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
          <channel>
            <title>Test Feed</title>
            <link>https://example.com/</link>
            <description>A test feed</description>
            <item>
              <title>First Post</title>
              <link>https://example.com/first-post</link>
            </item>
            <item>
              <title>Second Post</title>
              <link>https://example.com/second-post</link>
            </item>
          </channel>
        </rss>
        """;

    private const string EmptyFeedXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
          <channel>
            <title>Empty Feed</title>
            <link>https://example.com/</link>
            <description>A feed with no items</description>
          </channel>
        </rss>
        """;

    private const string FeedWithMissingFieldsXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0">
          <channel>
            <title>Partial Feed</title>
            <link>https://example.com/</link>
            <description>A feed with an incomplete item</description>
            <item>
              <description>No title or link here</description>
            </item>
          </channel>
        </rss>
        """;

    private static FeedRefreshService CreateService(HttpStatusCode statusCode, string content, string mediaType = "application/rss+xml")
    {
        var handler = new FakeHttpMessageHandler(statusCode, content, mediaType);
        var httpClient = new HttpClient(handler);
        var factory = new FakeHttpClientFactory(httpClient);
        return new FeedRefreshService(factory);
    }

    [Fact]
    public async Task RefreshAsync_WithValidFeed_ReturnsItemsWithTitleAndLink()
    {
        var service = CreateService(HttpStatusCode.OK, FeedWithItemsXml);

        var result = await service.RefreshAsync("https://example.com/feed.xml");

        Assert.True(result.Success);
        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items!.Count);
        Assert.Equal("First Post", result.Items[0].Title);
        Assert.Equal("https://example.com/first-post", result.Items[0].Link);
    }

    [Fact]
    public async Task RefreshAsync_WithEmptyFeed_ReturnsSuccessWithNoItems()
    {
        var service = CreateService(HttpStatusCode.OK, EmptyFeedXml);

        var result = await service.RefreshAsync("https://example.com/feed.xml");

        Assert.True(result.Success);
        Assert.Empty(result.Items!);
    }

    [Fact]
    public async Task RefreshAsync_WithItemMissingTitleOrLink_ReturnsBlankFieldsNotFailure()
    {
        var service = CreateService(HttpStatusCode.OK, FeedWithMissingFieldsXml);

        var result = await service.RefreshAsync("https://example.com/feed.xml");

        Assert.True(result.Success);
        var item = Assert.Single(result.Items!);
        Assert.Equal(string.Empty, item.Title);
        Assert.Equal(string.Empty, item.Link);
    }

    [Fact]
    public async Task RefreshAsync_WithNonFeedResponse_ReturnsFailure()
    {
        var service = CreateService(HttpStatusCode.OK, "<html><body>Not a feed</body></html>", "text/html");

        var result = await service.RefreshAsync("https://example.com/");

        Assert.False(result.Success);
        Assert.Equal("failed to load feed", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshAsync_WithUnreachableHost_ReturnsFailure()
    {
        var handler = new ThrowingHttpMessageHandler(new HttpRequestException("connection refused"));
        var httpClient = new HttpClient(handler);
        var service = new FeedRefreshService(new FakeHttpClientFactory(httpClient));

        var result = await service.RefreshAsync("https://unreachable.example.com/feed.xml");

        Assert.False(result.Success);
        Assert.Equal("failed to load feed", result.ErrorMessage);
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

    private sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw exception;
    }
}
