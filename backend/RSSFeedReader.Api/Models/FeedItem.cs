namespace RSSFeedReader.Api.Models;

// Title/Link may be blank when the source feed item omits them (FR-009).
public record FeedItem(string Title, string Link);
