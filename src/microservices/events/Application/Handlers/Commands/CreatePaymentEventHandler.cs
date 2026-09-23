using System.Globalization;
using CinemaAbyss.Events.Application.Abstractions;
using CinemaAbyss.Events.Application.Contracts.Requests;
using CinemaAbyss.Events.Application.Contracts.Responses;
using CinemaAbyss.Events.Application.Mapping;
using CinemaAbyss.Events.Domain.Entities;
using CinemaAbyss.Events.Domain.Enums;

namespace CinemaAbyss.Events.Application.Handlers.Commands;

/// <summary>
/// Records a payment outcome (completed, failed, …).
///
/// The payment id is the partitioning key, so the status changes of one
/// payment are consumed in the order they were produced.
/// </summary>
public sealed class CreatePaymentEventHandler(IEventPublisher publisher)
{
    public async Task<EventResponse> HandleAsync(PaymentEventRequest request, CancellationToken cancellationToken)
    {
        var key = request.PaymentId.ToString(CultureInfo.InvariantCulture);

        var cinemaEvent = CinemaEvent.Create(
            EventType.Payment,
            $"{key}-{request.Status}",
            request.Timestamp,
            request);

        var position = await publisher.PublishAsync(cinemaEvent, key, cancellationToken);

        return EventMapper.ToResponse(cinemaEvent, position);
    }
}
