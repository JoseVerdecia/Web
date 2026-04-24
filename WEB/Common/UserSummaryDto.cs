namespace WEB.Common;

public record UserSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public string? Email { get; init; }
}