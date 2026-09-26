using System.Globalization;
using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Application.Contracts.Requests;
using CinemaAbyss.Events.Application.Contracts.Responses;
using CinemaAbyss.Events.Application.Mapping;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Application.Handlers.Commands;

/// <summary>
/// Records something that happened to a movie (viewed, rated, added).
///
/// The movie id is the partitioning key, so every event of one movie is
/// consumed in the order it was produced.
/// </summary>
public sealed class CreateMovieEventHandler(IEventPublisher publisher, TimeProvider clock)
{
    public async Task<EventResponse> HandleAsync(MovieEventRequest request, CancellationToken cancellationToken)
    {
        var key = request.MovieId.ToString(CultureInfo.InvariantCulture);

        // The movie event carries no timestamp of its own: it happens when it is reported.
        var cinemaEvent = CinemaEvent.Create(
            EventType.Movie,
            $"{key}-{request.Action}",
            clock.GetUtcNow(),
            request);

        var position = await publisher.PublishAsync(cinemaEvent, key, cancellationToken);

        return EventMapper.ToResponse(cinemaEvent, position);
    }
}
