namespace CinemaAbyss.Proxy.Presentation.Endpoints;

/// <summary>
/// The proxy's own liveness probe (operation <c>getProxyHealth</c> in
/// api-specification.yaml). It is answered locally and never forwarded, so it
/// reports the proxy itself rather than a backend.
/// </summary>
internal sealed class HealthEndpoints : IEndpointModule
{
    public const string HealthyMessage = "Strangler Fig Proxy is healthy";

    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Text(HealthyMessage, "text/plain"))
            .WithName("GetProxyHealth");
    }
}
