using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Domain.Entities;

/// <summary>
/// The envelope every event travels in (schema <c>Event</c> in
/// api-specification.yaml): an identifier, the domain, the time it happened and
/// the domain-specific payload. The same shape is the Kafka message value, so a
/// consumer can route on <see cref="Type"/> without knowing every payload.
/// </summary>
public sealed record CinemaEvent(string Id, EventType Type, DateTimeOffset Timestamp, object Payload)
{
    /// <summary>
    /// Creates an event whose id reads like the specification's example
    /// (<c>movie-1-viewed</c>) followed by a random suffix, because the same
    /// user can view the same movie twice and an id must stay unique.
    /// </summary>
    /// <param name="subject">What happened, e.g. <c>1-viewed</c> for movie 1 being viewed.</param>
    public static CinemaEvent Create(EventType type, string subject, DateTimeOffset timestamp, object payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentNullException.ThrowIfNull(payload);

        var prefix = type.ToString().ToLowerInvariant();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        return new CinemaEvent($"{prefix}-{subject}-{suffix}", type, timestamp, payload);
    }
}
