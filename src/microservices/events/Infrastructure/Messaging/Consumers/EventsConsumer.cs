using System.Text.Json;
using CinemaAbyss.Events.Application.Handlers.Events;
using CinemaAbyss.Events.Domain.ValueObjects;
using Confluent.Kafka;

namespace CinemaAbyss.Events.Infrastructure.Messaging.Consumers;

/// <summary>
/// Broker adapter that reads the three event topics back. It contains no
/// logic of its own: it unwraps each message and hands it to
/// <see cref="ProcessEventHandler"/>, which keeps the use case testable without
/// a running broker.
///
/// Offsets are stored only after the handler returns, and committed by the
/// client's auto-commit, so delivery is at-least-once: a crash mid-message
/// replays it rather than losing it. A message that is not a valid event is
/// logged and skipped, so one bad record cannot stall the partition.
/// </summary>
public sealed class EventsConsumer(
    KafkaOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<EventsConsumer> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    // Consume() blocks, so the loop gets a thread of its own instead of holding
    // up host start-up or a thread-pool thread's async continuations.
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Factory.StartNew(
            () => ConsumeLoopAsync(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default).Unwrap();

    private async Task ConsumeLoopAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = options.Brokers,
            GroupId = options.ConsumerGroup,
            ClientId = options.ClientId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false,
            // The broker creates the topics (KAFKA_CREATE_TOPICS); this only
            // avoids an error if the consumer subscribes first.
            AllowAutoCreateTopics = true,
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => logger.LogWarning("Kafka consumer error {Code}: {Reason}", error.Code, error.Reason))
            .SetPartitionsAssignedHandler((_, partitions) => logger.LogInformation(
                "Assigned partitions: {Partitions}",
                string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition.Value}]"))))
            .Build();

        consumer.Subscribe(EventTopics.All);
        logger.LogInformation(
            "Consuming {Topics} as group {Group} from {Brokers}",
            string.Join(", ", EventTopics.All),
            options.ConsumerGroup,
            options.Brokers);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException exception)
                {
                    logger.LogWarning("Consume failed ({Reason}); retrying in {Delay}", exception.Error.Reason, RetryDelay);
                    await Task.Delay(RetryDelay, stoppingToken);
                    continue;
                }

                if (result?.Message is null)
                {
                    continue;
                }

                await ProcessAsync(result, stoppingToken);
                consumer.StoreOffset(result);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown.
        }
        finally
        {
            // Commits the stored offsets and leaves the group cleanly, so the
            // partitions are reassigned at once rather than after a session timeout.
            consumer.Close();
        }
    }

    private async Task ProcessAsync(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var position = new EventPosition(result.Topic, result.Partition.Value, result.Offset.Value);

        try
        {
            var cinemaEvent = EventSerializer.Deserialize(result.Message.Value);

            await using var scope = scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<ProcessEventHandler>();
            await handler.HandleAsync(cinemaEvent, position, cancellationToken);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Skipped message at {Position}: not a valid event envelope", position);
        }
    }
}
