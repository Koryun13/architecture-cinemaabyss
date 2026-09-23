namespace CinemaAbyss.Events.Infrastructure.Messaging;

/// <summary>Kafka connection settings, read from the environment.</summary>
public sealed record KafkaOptions(string Brokers, string ConsumerGroup, string ClientId)
{
    public const string BrokersVariable = "KAFKA_BROKERS";
    public const string ConsumerGroupVariable = "KAFKA_CONSUMER_GROUP";

    public static KafkaOptions FromConfiguration(IConfiguration configuration)
    {
        var brokers = configuration[BrokersVariable];
        var group = configuration[ConsumerGroupVariable];

        return new KafkaOptions(
            string.IsNullOrWhiteSpace(brokers) ? "localhost:9092" : brokers,
            string.IsNullOrWhiteSpace(group) ? "events-service" : group,
            ClientId: "events-service");
    }
}
