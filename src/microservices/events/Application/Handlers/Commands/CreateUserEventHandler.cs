using System.Globalization;
using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Application.Contracts.Requests;
using CinemaAbyss.Events.Application.Contracts.Responses;
using CinemaAbyss.Events.Application.Mapping;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Application.Handlers.Commands;

/// <summary>
/// Records a user action (registered, logged in, …).
///
/// The user id is the partitioning key, so every event of one user is consumed
/// in the order it was produced.
/// </summary>
public sealed class CreateUserEventHandler(IEventPublisher publisher)
{
    public async Task<EventResponse> HandleAsync(UserEventRequest request, CancellationToken cancellationToken)
    {
        var key = request.UserId.ToString(CultureInfo.InvariantCulture);

        var cinemaEvent = CinemaEvent.Create(
            EventType.User,
            $"{key}-{request.Action}",
            request.Timestamp,
            request);

        var position = await publisher.PublishAsync(cinemaEvent, key, cancellationToken);

        return EventMapper.ToResponse(cinemaEvent, position);
    }
}
