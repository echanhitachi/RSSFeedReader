using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

// In-memory store per FR-006; lock-protected list since ASP.NET Core can process requests concurrently.
public class InMemorySubscriptionService : ISubscriptionService
{
    private readonly List<Subscription> _subscriptions = new();
    private readonly object _lock = new();

    public Subscription? Add(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var subscription = new Subscription(url);
        lock (_lock)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    public IReadOnlyList<Subscription> GetAll()
    {
        lock (_lock)
        {
            return _subscriptions.ToList();
        }
    }
}
