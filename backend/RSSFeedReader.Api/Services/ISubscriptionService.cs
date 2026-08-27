using RSSFeedReader.Api.Models;

namespace RSSFeedReader.Api.Services;

public interface ISubscriptionService
{
    // Callers must pass an already-validated (non-empty, feed-verified) url; validation lives in IFeedValidationService.
    Subscription Add(string url);

    // Returns false when id does not exist; callers MUST treat this as a no-op, not an error (FR-010).
    bool Remove(Guid id);

    IReadOnlyList<Subscription> GetAll();
}
