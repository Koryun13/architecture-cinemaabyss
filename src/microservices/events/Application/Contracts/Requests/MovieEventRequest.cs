namespace CinemaAbyss.Events.Application.Contracts.Requests;

/// <summary>
/// Schema <c>MovieEvent</c>. Parameters without a default are required: the JSON
/// contract rejects a body that omits them or sends null (see
/// <c>ServiceDefaults.ApplyJsonContract</c>).
/// </summary>
public sealed record MovieEventRequest(
    int MovieId,
    string Title,
    string Action,
    int? UserId = null,
    double? Rating = null,
    IReadOnlyList<string>? Genres = null,
    string? Description = null);
