using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Application.Contracts.Responses;

/// <summary>Schema <c>Event</c>.</summary>
public sealed record EventDto(string Id, EventType Type, DateTimeOffset Timestamp, object Payload);
