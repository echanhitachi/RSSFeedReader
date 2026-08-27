using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

// In-memory store per FR-006; lock-protected list since ASP.NET Core can process requests concurrently.
public class InMemorySubscriptionService : ISubscriptionService
{
    private readonly List<Subscription> _subscriptions = new();
    private readonly object _lock = new();

    public Subscription Add(string url)
    {
        var subscription = new Subscription(Guid.NewGuid(), url);
        lock (_lock)
        {
            _subscriptions.Add(subscription);
        }

        return subscription;
    }

    public bool Remove(Guid id)
    {
        lock (_lock)
        {
            return _subscriptions.RemoveAll(s => s.Id == id) > 0;
        }
    }

    public IReadOnlyList<Subscription> GetAll()
    {
        lock (_lock)
        {
            return _subscriptions.ToList();
        }
    }
}
