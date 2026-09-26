using CinemaAbyss.Proxy.Domain.Policies;

namespace CinemaAbyss.Proxy.Infrastructure.Configuration;

/// <summary>
/// Reads the proxy settings from the environment variables that
/// docker-compose.yml and the Kubernetes ConfigMap provide.
///
/// Invalid values fail the service at start-up with a message naming the
/// variable, rather than silently routing traffic somewhere unexpected.
/// </summary>
public static class ProxySettings
{
    public const string MonolithUrl = "MONOLITH_URL";
    public const string MoviesServiceUrl = "MOVIES_SERVICE_URL";
    public const string EventsServiceUrl = "EVENTS_SERVICE_URL";
    public const string GradualMigration = "GRADUAL_MIGRATION";
    public const string MoviesMigrationPercent = "MOVIES_MIGRATION_PERCENT";

    public static UpstreamOptions ReadUpstreams(IConfiguration configuration) => new(
        ReadUrl(configuration, MonolithUrl, "http://localhost:8080"),
        ReadUrl(configuration, MoviesServiceUrl, "http://localhost:8081"),
        ReadUrl(configuration, EventsServiceUrl, "http://localhost:8082"));

    public static MoviesMigrationPolicy ReadMigrationPolicy(IConfiguration configuration)
    {
        var gradualValue = configuration[GradualMigration];
        var gradual = true;
        if (!string.IsNullOrWhiteSpace(gradualValue) && !bool.TryParse(gradualValue, out gradual))
        {
            throw new InvalidOperationException(
                $"{GradualMigration} must be \"true\" or \"false\", got \"{gradualValue}\".");
        }

        var percentValue = configuration[MoviesMigrationPercent];
        var percent = 0;
        if (!string.IsNullOrWhiteSpace(percentValue)
            && (!int.TryParse(percentValue, out percent)
                || percent is < MoviesMigrationPolicy.MinPercent or > MoviesMigrationPolicy.MaxPercent))
        {
            throw new InvalidOperationException(
                $"{MoviesMigrationPercent} must be an integer between {MoviesMigrationPolicy.MinPercent} " +
                $"and {MoviesMigrationPolicy.MaxPercent}, got \"{percentValue}\".");
        }

        return new MoviesMigrationPolicy(gradual, percent);
    }

    private static Uri ReadUrl(IConfiguration configuration, string key, string fallback)
    {
        var value = configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            value = fallback;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"{key} must be an absolute http(s) URL, got \"{value}\".");
        }

        return uri;
    }
}
