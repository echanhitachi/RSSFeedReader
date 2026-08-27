using System.Net.Http.Json;

namespace RSSFeedReader.UI.Services;

public class SubscriptionApiClient(HttpClient httpClient)
{
    public async Task<bool> AddSubscriptionAsync(string url)
    {
        var response = await httpClient.PostAsJsonAsync("api/subscriptions", new { url });
        return response.IsSuccessStatusCode;
    }
}
