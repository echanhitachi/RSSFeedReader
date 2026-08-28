using System.ServiceModel.Syndication;
using System.Xml;
using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public class FeedRefreshService(IHttpClientFactory httpClientFactory) : IFeedRefreshService
{
    private static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(5);

    public async Task<FeedRefreshResult> RefreshAsync(string url)
    {
        using var client = httpClientFactory.CreateClient(nameof(FeedRefreshService));
        client.Timeout = FetchTimeout;

        try
        {
            await using var stream = await client.GetStreamAsync(url);
            using var xmlReader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(xmlReader);

            if (feed is null)
            {
                return new FeedRefreshResult(false, null, "failed to load feed");
            }

            var items = feed.Items
                .Select(item => new FeedItem(item.Title?.Text ?? string.Empty, item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty))
                .ToList();

            return new FeedRefreshResult(true, items, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException)
        {
            return new FeedRefreshResult(false, null, "failed to load feed");
        }
    }
}
