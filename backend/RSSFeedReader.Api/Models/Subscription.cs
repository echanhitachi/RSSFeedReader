namespace RSSFeedReader.Api.Models;

// Id enables removing a specific entry even when duplicate Urls exist (FR-007).
public record Subscription(Guid Id, string Url);
