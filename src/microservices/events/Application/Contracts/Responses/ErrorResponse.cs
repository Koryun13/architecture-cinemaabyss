namespace CinemaAbyss.Events.Application.Contracts.Responses;

/// <summary>Schema <c>Error</c>, returned with every 4xx/5xx response.</summary>
public sealed record ErrorResponse(string Error);
