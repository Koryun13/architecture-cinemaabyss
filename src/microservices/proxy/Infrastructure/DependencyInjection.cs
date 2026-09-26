using CinemaAbyss.Proxy.Infrastructure.Configuration;
using CinemaAbyss.Proxy.Infrastructure.Routing;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.Transforms;

namespace CinemaAbyss.Proxy.Infrastructure;

/// <summary>
/// Wires the reverse proxy: the routing table built from the environment and
/// the Strangler Fig policy that splits the movies domain.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Names the backend that served a proxied response, which makes the traffic split observable from the outside.</summary>
    public const string UpstreamHeader = "X-Upstream-Service";

    public static IServiceCollection AddProxyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var upstreams = ProxySettings.ReadUpstreams(configuration);
        var migrationPolicy = ProxySettings.ReadMigrationPolicy(configuration);

        services.AddSingleton(upstreams);
        services.AddSingleton(migrationPolicy);
        services.AddSingleton<ILoadBalancingPolicy, StranglerFigLoadBalancingPolicy>();

        services
            .AddReverseProxy()
            .LoadFromMemory(RouteTable.BuildRoutes(), RouteTable.BuildClusters(upstreams))
            .AddTransforms(transforms => transforms.AddResponseTransform(context =>
            {
                var destination = context.HttpContext.Features.Get<IReverseProxyFeature>()?.ProxiedDestination;
                if (destination is not null)
                {
                    context.HttpContext.Response.Headers[UpstreamHeader] = destination.DestinationId;
                }

                return ValueTask.CompletedTask;
            }));

        return services;
    }
}
