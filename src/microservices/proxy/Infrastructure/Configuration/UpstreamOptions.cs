namespace CinemaAbyss.Proxy.Infrastructure.Configuration;

/// <summary>Base addresses of the backends behind the proxy.</summary>
public sealed record UpstreamOptions(Uri MonolithUrl, Uri MoviesServiceUrl, Uri EventsServiceUrl);
