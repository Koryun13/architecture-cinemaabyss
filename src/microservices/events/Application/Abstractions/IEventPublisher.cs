using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.ValueObjects;

namespace CinemaAbyss.Events.Application.Abstractions;

/// <summary>Outbound port: appends an event to the event log.</summary>
public interface IEventPublisher
{
    /// <param name="key">
    /// Partitioning key. Events with the same key land in the same partition
    /// and are therefore consumed in the order they were produced.
    /// </param>
    /// <returns>The position the broker acknowledged.</returns>
    /// <exception cref="EventPublishFailedException">The broker did not acknowledge the event.</exception>
    Task<EventPosition> PublishAsync(CinemaEvent cinemaEvent, string key, CancellationToken cancellationToken);
}
