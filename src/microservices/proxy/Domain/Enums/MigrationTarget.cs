namespace CinemaAbyss.Proxy.Domain.Enums;

/// <summary>Where a request for the movies domain is served.</summary>
public enum MigrationTarget
{
    /// <summary>The legacy Go monolith that still owns the domain.</summary>
    Monolith,

    /// <summary>The extracted movies microservice.</summary>
    MoviesService,
}
