using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Infrastructure.Messaging;
using CinemaAbyss.Events.Infrastructure.Messaging.Consumers;

namespace CinemaAbyss.Events.Infrastructure;

/// <summary>
/// Binds the ports declared by the inner layers to their concrete adapters.
/// This is the only place where the direction of the dependency is resolved.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddEventsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(KafkaOptions.FromConfiguration(configuration));

        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        services.AddHostedService<EventsConsumer>();

        return services;
    }
}
