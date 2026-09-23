using CinemaAbyss.Proxy.Domain.Enums;
using CinemaAbyss.Proxy.Domain.Policies;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace CinemaAbyss.Proxy.Infrastructure.Routing;

/// <summary>
/// Picks the backend for a movies request according to the
/// <see cref="MoviesMigrationPolicy"/> feature flag.
///
/// The movies cluster has two destinations — the monolith and movies-service —
/// and choosing between them is exactly what a YARP load-balancing policy is
/// for, so the Strangler Fig split needs no custom forwarding code.
/// </summary>
public sealed class StranglerFigLoadBalancingPolicy(
    MoviesMigrationPolicy migrationPolicy,
    ILogger<StranglerFigLoadBalancingPolicy> logger) : ILoadBalancingPolicy
{
    public const string PolicyName = "StranglerFig";

    public string Name => PolicyName;

    public DestinationState? PickDestination(
        HttpContext context,
        ClusterState cluster,
        IReadOnlyList<DestinationState> availableDestinations)
    {
        var roll = Random.Shared.Next(MoviesMigrationPolicy.MaxPercent);
        var target = migrationPolicy.Choose(roll);

        var destinationId = target == MigrationTarget.MoviesService
            ? RouteTable.MoviesServiceDestination
            : RouteTable.MonolithDestination;

        // YARP consults the policy only when two or more destinations are
        // available, so both backends are in the list here.
        var destination = availableDestinations.First(d => d.DestinationId == destinationId);

        logger.LogInformation(
            "{Method} {Path}{Query} -> {Destination} (roll {Roll}, {Policy})",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            destinationId,
            roll,
            migrationPolicy);

        return destination;
    }
}
