using CinemaAbyss.Events.Application.Contracts.Responses;

namespace CinemaAbyss.Events.Presentation.Endpoints;

/// <summary>
/// Liveness probe (operation <c>getEventsServiceHealth</c>). It sits under
/// /api/events so that the ingress rule for the events service exposes it too.
/// </summary>
internal sealed class HealthEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/health", () => TypedResults.Ok(new HealthResponse(true)))
            .WithTags("Health")
            .WithName("GetEventsServiceHealth");
    }
}
