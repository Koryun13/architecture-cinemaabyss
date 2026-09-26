using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Infrastructure.Messaging;

/// <summary>
/// One topic per domain. The names match KAFKA_CREATE_TOPICS in
/// docker-compose.yml and in the Kubernetes Kafka manifest.
/// </summary>
public static class EventTopics
{
    public const string Movie = "movie-events";
    public const string User = "user-events";
    public const string Payment = "payment-events";

    public static IReadOnlyList<string> All { get; } = [Movie, User, Payment];

    public static string For(EventType type) => type switch
    {
        EventType.Movie => Movie,
        EventType.User => User,
        EventType.Payment => Payment,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "No topic is defined for this event type."),
    };
}
