using System.Net.Http.Json;

namespace RSSFeedReader.UI.Services;

public record Subscription(Guid Id, string Url);

public record AddSubscriptionResult(bool Success, Subscription? Subscription, string? ErrorMessage);

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

    private record ErrorResponse(string Error);
}
