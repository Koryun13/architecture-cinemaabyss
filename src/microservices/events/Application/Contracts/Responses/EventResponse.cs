namespace CinemaAbyss.Events.Application.Contracts.Responses;

/// <summary>Schema <c>EventResponse</c>: the event as stored and where Kafka put it.</summary>
public sealed record EventResponse(string Status, int Partition, long Offset, EventDto Event)
{
    public const string Success = "success";
}
