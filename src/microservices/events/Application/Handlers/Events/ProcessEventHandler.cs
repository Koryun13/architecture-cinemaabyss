using System.Text.Json;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.ValueObjects;

namespace CinemaAbyss.Events.Application.Handlers.Events;

/// <summary>
/// Handles an event read back from Kafka. For the MVP, processing means
/// recording the event in the service log — enough to prove the round trip
/// API -> producer -> topic -> consumer, and the place where real reactions
/// (recommendations, analytics, notifications) would be added.
/// </summary>
public sealed class ProcessEventHandler(ILogger<ProcessEventHandler> logger)
{
    public Task HandleAsync(CinemaEvent cinemaEvent, EventPosition position, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processed {EventType} event {EventId} from {Position}: {Payload}",
            cinemaEvent.Type,
            cinemaEvent.Id,
            position,
            cinemaEvent.Payload is JsonElement json ? json.GetRawText() : cinemaEvent.Payload);

        return Task.CompletedTask;
    }
}
