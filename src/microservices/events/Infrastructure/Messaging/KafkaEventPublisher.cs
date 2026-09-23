using System.Text;
using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.ValueObjects;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Infrastructure.Messaging;

/// <summary>
/// Kafka producer adapter. One producer instance is shared for the lifetime
/// of the service: it is thread-safe and batches internally, and creating one
/// per request would open a new broker connection each time.
///
/// <list type="bullet">
///   <item><description><b>acks=all + idempotence</b> — an acknowledged event is on every in-sync replica and a retry cannot duplicate it.</description></item>
///   <item><description><b>message.timeout.ms</b> — bounded, so an unreachable broker turns into a 500 within seconds instead of the client default of five minutes.</description></item>
/// </list>
/// </summary>
public sealed class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private const int MessageTimeoutMs = 5000;
    private static readonly TimeSpan FlushTimeout = TimeSpan.FromSeconds(5);

    private readonly IProducer<string, string> producer;
    private readonly ILogger<KafkaEventPublisher> logger;

    public KafkaEventPublisher(KafkaOptions options, ILogger<KafkaEventPublisher> logger)
    {
        this.logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = options.Brokers,
            ClientId = options.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = MessageTimeoutMs,
        };

        producer = new ProducerBuilder<string, string>(config)
            .SetLogHandler((_, message) => logger.LogDebug("librdkafka {Facility}: {Message}", message.Facility, message.Message))
            .SetErrorHandler((_, error) => logger.LogWarning("Kafka producer error {Code}: {Reason}", error.Code, error.Reason))
            .Build();
    }

    public async Task<EventPosition> PublishAsync(
        CinemaEvent cinemaEvent,
        string key,
        CancellationToken cancellationToken)
    {
        var topic = EventTopics.For(cinemaEvent.Type);
        var message = new Message<string, string>
        {
            Key = key,
            Value = EventSerializer.Serialize(cinemaEvent),
            Headers = new Headers
            {
                { "event-id", Encoding.UTF8.GetBytes(cinemaEvent.Id) },
                { "event-type", Encoding.UTF8.GetBytes(cinemaEvent.Type.ToString().ToLowerInvariant()) },
            },
        };

        try
        {
            var result = await producer.ProduceAsync(topic, message, cancellationToken);
            var position = new EventPosition(result.Topic, result.Partition.Value, result.Offset.Value);

            logger.LogInformation(
                "Produced {EventType} event {EventId} (key {Key}) to {Position}",
                cinemaEvent.Type,
                cinemaEvent.Id,
                key,
                position);

            return position;
        }
        catch (ProduceException<string, string> exception)
        {
            logger.LogError(
                exception,
                "Failed to produce {EventType} event {EventId} to {Topic}: {Reason}",
                cinemaEvent.Type,
                cinemaEvent.Id,
                topic,
                exception.Error.Reason);

            throw new EventPublishFailedException(
                $"Failed to publish the event to Kafka topic '{topic}': {exception.Error.Reason}",
                exception);
        }
    }

    public void Dispose()
    {
        // Deliver whatever is still queued before the process exits.
        producer.Flush(FlushTimeout);
        producer.Dispose();
    }
}
