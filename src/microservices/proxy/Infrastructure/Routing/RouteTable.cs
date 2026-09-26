using CinemaAbyss.Proxy.Infrastructure.Configuration;
using Yarp.ReverseProxy.Configuration;

namespace CinemaAbyss.Proxy.Infrastructure.Routing;

/// <summary>
/// The proxy's routing table, built from the upstream addresses.
///
/// <list type="bullet">
///   <item><description><c>/api/movies/**</c> — the domain being strangled: split between monolith and movies-service by <see cref="StranglerFigLoadBalancingPolicy"/>.</description></item>
///   <item><description><c>/api/events/**</c> — the events microservice.</description></item>
///   <item><description>everything else (<c>/api/users</c>, <c>/api/payments</c>, <c>/api/subscriptions</c>, …) — still the monolith. This is the facade half of the pattern: a domain leaves the monolith by gaining its own route above this one.</description></item>
/// </list>
/// </summary>
public static class RouteTable
{
    public const string MoviesCluster = "movies";
    public const string EventsCluster = "events";
    public const string MonolithCluster = "monolith";

    /// <summary>Destination ids double as the service names shown in logs and in the X-Upstream-Service header.</summary>
    public const string MonolithDestination = "monolith";
    public const string MoviesServiceDestination = "movies-service";
    public const string EventsServiceDestination = "events-service";

    // Lower order wins. The monolith catch-all must lose to every specific route,
    // including the proxy's own /health endpoint (order 0).
    private const int SpecificRouteOrder = 0;
    private const int FallbackRouteOrder = 1000;

    public static IReadOnlyList<RouteConfig> BuildRoutes() =>
    [
        new RouteConfig
        {
            RouteId = "movies",
            ClusterId = MoviesCluster,
            Order = SpecificRouteOrder,
            // A catch-all segment is optional, so this also matches /api/movies itself.
            Match = new RouteMatch { Path = "/api/movies/{**catch-all}" },
        },
        new RouteConfig
        {
            RouteId = "events",
            ClusterId = EventsCluster,
            Order = SpecificRouteOrder,
            Match = new RouteMatch { Path = "/api/events/{**catch-all}" },
        },
        new RouteConfig
        {
            RouteId = "monolith",
            ClusterId = MonolithCluster,
            Order = FallbackRouteOrder,
            Match = new RouteMatch { Path = "/{**catch-all}" },
        },
    ];

    public static IReadOnlyList<ClusterConfig> BuildClusters(UpstreamOptions upstreams) =>
    [
        new ClusterConfig
        {
            ClusterId = MoviesCluster,
            LoadBalancingPolicy = StranglerFigLoadBalancingPolicy.PolicyName,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [MonolithDestination] = new() { Address = upstreams.MonolithUrl.ToString() },
                [MoviesServiceDestination] = new() { Address = upstreams.MoviesServiceUrl.ToString() },
            },
        },
        new ClusterConfig
        {
            ClusterId = EventsCluster,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [EventsServiceDestination] = new() { Address = upstreams.EventsServiceUrl.ToString() },
            },
        },
        new ClusterConfig
        {
            ClusterId = MonolithCluster,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [MonolithDestination] = new() { Address = upstreams.MonolithUrl.ToString() },
            },
        },
    ];
}
