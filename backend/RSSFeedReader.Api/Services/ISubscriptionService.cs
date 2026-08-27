using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public interface ISubscriptionService
{
    // Returns null when url is empty/whitespace (rejected per FR-008); duplicates are allowed (FR-007).
    Subscription? Add(string url);

    IReadOnlyList<Subscription> GetAll();
}
