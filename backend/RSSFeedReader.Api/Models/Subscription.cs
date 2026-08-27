namespace RSSFeedReader.Api.Models;

// Per data-model.md: MVP subscription has only a raw, unvalidated URL string.
public record Subscription(string Url);
