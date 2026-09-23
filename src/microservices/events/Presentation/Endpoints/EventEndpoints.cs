using CinemaAbyss.Events.Application.Contracts.Requests;
using CinemaAbyss.Events.Application.Contracts.Responses;
using CinemaAbyss.Events.Application.Handlers.Commands;

namespace CinemaAbyss.Events.Presentation.Endpoints;

/// <summary>
/// Event intake: each call produces one event to its Kafka topic and answers
/// with the partition and offset the broker assigned. The same service consumes
/// the event back asynchronously (see <c>EventsConsumer</c>).
/// </summary>
internal sealed class EventEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events").WithTags("Events");

        group.MapPost("/movie", async (
                MovieEventRequest request,
                CreateMovieEventHandler handler,
                CancellationToken ct) => Created(await handler.HandleAsync(request, ct)))
            .WithName("CreateMovieEvent")
            .WithSummary("Create a movie event")
            .ProducesEventResponses();

        group.MapPost("/user", async (
                UserEventRequest request,
                CreateUserEventHandler handler,
                CancellationToken ct) => Created(await handler.HandleAsync(request, ct)))
            .WithName("CreateUserEvent")
            .WithSummary("Create a user event")
            .ProducesEventResponses();

        group.MapPost("/payment", async (
                PaymentEventRequest request,
                CreatePaymentEventHandler handler,
                CancellationToken ct) => Created(await handler.HandleAsync(request, ct)))
            .WithName("CreatePaymentEvent")
            .WithSummary("Create a payment event")
            .ProducesEventResponses();
    }

    // Events are not addressable resources, so the 201 carries no Location.
    private static IResult Created(EventResponse response) => TypedResults.Created((string?)null, response);
}
