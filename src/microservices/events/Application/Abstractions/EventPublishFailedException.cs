namespace CinemaAbyss.Events.Application.Abstractions;

/// <summary>
/// Raised by an <see cref="IEventPublisher"/> when the broker does not
/// acknowledge an event. It keeps broker-specific exceptions out of the
/// presentation layer, which only needs to know that publishing failed.
/// </summary>
public sealed class EventPublishFailedException(string message, Exception innerException)
    : Exception(message, innerException);
