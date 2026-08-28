using System.Net.Http.Json;

namespace RSSFeedReader.UI.Services;

public record Subscription(Guid Id, string Url);

public record AddSubscriptionResult(bool Success, Subscription? Subscription, string? ErrorMessage);

public record FeedItem(string Title, string Link);

public record RefreshResult(bool Success, List<FeedItem>? Items, string? ErrorMessage);

public class SubscriptionApiClient(HttpClient httpClient)
{
    public async Task<AddSubscriptionResult> AddSubscriptionAsync(string url)
    {
        var response = await httpClient.PostAsJsonAsync("api/subscriptions", new { url });
        if (response.IsSuccessStatusCode)
        {
            var subscription = await response.Content.ReadFromJsonAsync<Subscription>();
            return new AddSubscriptionResult(true, subscription, null);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return new AddSubscriptionResult(false, null, error?.Error ?? "Failed to add subscription.");
    }

    public async Task<List<Subscription>> GetSubscriptionsAsync()
    {
        var result = await httpClient.GetFromJsonAsync<List<Subscription>>("api/subscriptions");
        return result ?? [];
    }

    public async Task RemoveSubscriptionAsync(Guid id)
    {
        await httpClient.DeleteAsync($"api/subscriptions/{id}");
    }

    public async Task<RefreshResult> RefreshSubscriptionAsync(Guid id)
    {
        var response = await httpClient.PostAsync($"api/subscriptions/{id}/refresh", null);
        if (response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadFromJsonAsync<RefreshResponseBody>();
            return new RefreshResult(true, body?.Items ?? [], null);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return new RefreshResult(false, null, error?.Error ?? "Failed to load feed.");
    }

    private record ErrorResponse(string Error);

    private record RefreshResponseBody(List<FeedItem> Items);
}
