using CinemaAbyss.Events.Application.Contracts.Responses;

namespace CinemaAbyss.Events.Presentation.Endpoints;

/// <summary>OpenAPI response metadata shared by the event intake endpoints.</summary>
internal static class EndpointMetadataExtensions
{
    public static RouteHandlerBuilder ProducesEventResponses(this RouteHandlerBuilder builder) => builder
        .Produces<EventResponse>(StatusCodes.Status201Created)
        .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ErrorResponse>(StatusCodes.Status500InternalServerError);
}
