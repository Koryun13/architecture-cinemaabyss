namespace CinemaAbyss.Events.Domain.ValueObjects;

/// <summary>Where an event is stored in the log: topic, partition and offset.</summary>
public sealed record EventPosition(string Topic, int Partition, long Offset)
{
    public override string ToString() => $"{Topic}[{Partition}]@{Offset}";
}
