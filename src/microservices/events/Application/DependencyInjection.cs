using CinemaAbyss.Events.Application.Handlers.Commands;
using CinemaAbyss.Events.Application.Handlers.Events;

namespace CinemaAbyss.Events.Application;

/// <summary>
/// Registers the use cases of this service. The application layer owns its own
/// registration so the composition root does not have to know them one by one.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddEventsApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<CreateMovieEventHandler>();
        services.AddScoped<CreateUserEventHandler>();
        services.AddScoped<CreatePaymentEventHandler>();
        services.AddScoped<ProcessEventHandler>();

        return services;
    }
}
