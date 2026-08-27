namespace RSSFeedReader.Api.Services;

public record FeedValidationResult(bool IsValid, string? ErrorMessage);

public interface IFeedValidationService
{
    Task<FeedValidationResult> ValidateAsync(string url);
}
