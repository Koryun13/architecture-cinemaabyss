using System.Reflection;
using CinemaAbyss.Proxy.Presentation;

namespace CinemaAbyss.Proxy.Infrastructure.Hosting;

/// <summary>
/// Host configuration shared by the service's composition root: the listening
/// port and discovery of the presentation modules.
/// </summary>
public static class ServiceDefaults
{
    private const string PortVariable = "PORT";

    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.UseServicePort();
        return builder;
    }

    /// <summary>
    /// Binds Kestrel to the <c>PORT</c> variable that docker-compose.yml and the
    /// Kubernetes manifests set. It is applied as the HTTP_PORTS host setting,
    /// which overrides the base image's ASPNETCORE_HTTP_PORTS=8080 without the
    /// "overriding addresses" warning that UseUrls would log. When PORT is unset
    /// (a local run) the launch profile's applicationUrl is used instead.
    /// </summary>
    public static WebApplicationBuilder UseServicePort(this WebApplicationBuilder builder)
    {
        var port = builder.Configuration[PortVariable];
        if (string.IsNullOrWhiteSpace(port))
        {
            return builder;
        }

        if (!int.TryParse(port, out var number) || number is < 1 or > 65535)
        {
            throw new InvalidOperationException($"{PortVariable} must be a TCP port number, got \"{port}\".");
        }

        builder.WebHost.UseSetting(WebHostDefaults.HttpPortsKey, port);
        return builder;
    }

    /// <summary>
    /// Discovers the presentation modules of the service and registers them, so
    /// adding a resource group means adding a class, not editing Program.cs.
    /// </summary>
    public static IServiceCollection AddEndpointModules(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes()
                     .Where(t => t is { IsAbstract: false, IsInterface: false }
                                 && typeof(IEndpointModule).IsAssignableFrom(t)))
        {
            services.AddSingleton(typeof(IEndpointModule), type);
        }

        return services;
    }

    /// <summary>Maps every registered presentation module.</summary>
    public static WebApplication MapEndpointModules(this WebApplication app)
    {
        foreach (var module in app.Services.GetServices<IEndpointModule>())
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}
