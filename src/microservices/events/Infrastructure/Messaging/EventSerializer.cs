using System.Text.Json;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Infrastructure.Hosting;

namespace CinemaAbyss.Events.Infrastructure.Messaging;

/// <summary>
/// Kafka message values are the <see cref="CinemaEvent"/> envelope as JSON, in
/// the same snake_case contract as the HTTP API, so what a consumer reads from
/// a topic is exactly what the producer's caller got back.
/// </summary>
public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Serialize(CinemaEvent cinemaEvent) => JsonSerializer.Serialize(cinemaEvent, Options);

    /// <exception cref="JsonException">The value is not a valid event envelope.</exception>
    public static CinemaEvent Deserialize(string value)
        => JsonSerializer.Deserialize<CinemaEvent>(value, Options)
           ?? throw new JsonException("The message value is the JSON literal null.");

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        ServiceDefaults.ApplyJsonContract(options);
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
