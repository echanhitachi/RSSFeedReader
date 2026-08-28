using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public record FeedRefreshResult(bool Success, IReadOnlyList<FeedItem>? Items, string? ErrorMessage);

public interface IFeedRefreshService
{
    Task<FeedRefreshResult> RefreshAsync(string url);
}
