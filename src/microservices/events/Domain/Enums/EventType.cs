namespace CinemaAbyss.Events.Domain.Enums;

/// <summary>The business domain an event belongs to; each has its own Kafka topic.</summary>
public enum EventType
{
    Movie,
    User,
    Payment,
}
