using CinemaAbyss.Events.Application.Contracts.Responses;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.ValueObjects;

namespace CinemaAbyss.Events.Application.Mapping;

/// <summary>Projects a stored event onto its published shapes.</summary>
public static class EventMapper
{
    public static EventDto ToDto(CinemaEvent cinemaEvent) => new(
        cinemaEvent.Id,
        cinemaEvent.Type,
        cinemaEvent.Timestamp,
        cinemaEvent.Payload);

    public static EventResponse ToResponse(CinemaEvent cinemaEvent, EventPosition position) => new(
        EventResponse.Success,
        position.Partition,
        position.Offset,
        ToDto(cinemaEvent));
}
