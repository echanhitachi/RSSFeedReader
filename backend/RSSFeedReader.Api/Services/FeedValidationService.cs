using System.ServiceModel.Syndication;
using System.Xml;

namespace RSSFeedReader.Api.Services;

public class FeedValidationService(IHttpClientFactory httpClientFactory) : IFeedValidationService
{
    private static readonly TimeSpan FetchTimeout = TimeSpan.FromSeconds(5);

    public async Task<FeedValidationResult> ValidateAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return new FeedValidationResult(false, "url is not a valid http/https URL");
        }

        using var client = httpClientFactory.CreateClient(nameof(FeedValidationService));
        client.Timeout = FetchTimeout;

        try
        {
            await using var stream = await client.GetStreamAsync(uri);
            using var xmlReader = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(xmlReader);

            return feed is null || string.IsNullOrWhiteSpace(feed.Title?.Text)
                ? new FeedValidationResult(false, "could not verify a feed at this url")
                : new FeedValidationResult(true, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or XmlException)
        {
            return new FeedValidationResult(false, "could not verify a feed at this url");
        }
    }
}
