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

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("https://devblogs.microsoft.com/dotnet/feed/", result.Url);
    }

    [Fact]
    public void Add_WithDuplicateUrl_AllowsBothEntriesWithDistinctIds()
    {
        var service = new InMemorySubscriptionService();

        var first = service.Add("https://example.com/feed.xml");
        var second = service.Add("https://example.com/feed.xml");

        Assert.Equal(2, service.GetAll().Count);
        Assert.NotEqual(first.Id, second.Id);
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

    [Fact]
    public void Remove_WithMatchingId_RemovesOnlyThatEntry()
    {
        var service = new InMemorySubscriptionService();
        var first = service.Add("https://example.com/feed.xml");
        var second = service.Add("https://example.com/feed.xml");

        var removed = service.Remove(first.Id);

        Assert.True(removed);
        var remaining = service.GetAll();
        Assert.Single(remaining);
        Assert.Equal(second.Id, remaining[0].Id);
    }

    [Fact]
    public void Remove_WithNonExistentId_ReturnsFalseWithoutThrowing()
    {
        var service = new InMemorySubscriptionService();

        var removed = service.Remove(Guid.NewGuid());

        Assert.False(removed);
    }
}
