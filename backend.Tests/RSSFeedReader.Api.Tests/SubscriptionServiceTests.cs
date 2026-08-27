using RSSFeedReader.Api.Services;
using Xunit;

namespace RSSFeedReader.Api.Tests;

public class SubscriptionServiceTests
{
    [Fact]
    public void Add_WithNonEmptyUrl_ReturnsSubscription()
    {
        var service = new InMemorySubscriptionService();

        var result = service.Add("https://devblogs.microsoft.com/dotnet/feed/");

        Assert.NotNull(result);
        Assert.Equal("https://devblogs.microsoft.com/dotnet/feed/", result!.Url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Add_WithBlankUrl_ReturnsNullAndDoesNotAdd(string? url)
    {
        var service = new InMemorySubscriptionService();

        var result = service.Add(url!);

        Assert.Null(result);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Add_WithDuplicateUrl_AllowsBothEntries()
    {
        var service = new InMemorySubscriptionService();

        service.Add("https://example.com/feed.xml");
        service.Add("https://example.com/feed.xml");

        Assert.Equal(2, service.GetAll().Count);
    }

    [Fact]
    public void GetAll_WithNoSubscriptions_ReturnsEmptyCollection()
    {
        var service = new InMemorySubscriptionService();

        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void GetAll_AfterAdds_ReturnsEntriesInInsertionOrder()
    {
        var service = new InMemorySubscriptionService();

        service.Add("https://example.com/first.xml");
        service.Add("https://example.com/second.xml");

        var result = service.GetAll();

        Assert.Equal(2, result.Count);
        Assert.Equal("https://example.com/first.xml", result[0].Url);
        Assert.Equal("https://example.com/second.xml", result[1].Url);
    }
}
