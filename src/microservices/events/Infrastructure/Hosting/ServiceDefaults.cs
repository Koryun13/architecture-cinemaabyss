using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CinemaAbyss.Events.Presentation;
using CinemaAbyss.Events.Presentation.ErrorHandling;
using Microsoft.AspNetCore.Http.Json;
using Scalar.AspNetCore;

namespace CinemaAbyss.Events.Infrastructure.Hosting;

/// <summary>
/// Cross-cutting host configuration: the listening port, one JSON contract,
/// OpenAPI and error responses. Keeping it here is what lets the composition
/// root stay a few lines long.
/// </summary>
public static class ServiceDefaults
{
    private const string PortVariable = "PORT";

    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder, string serviceTitle)
    {
        builder.UseServicePort();

        builder.Services.Configure<JsonOptions>(options => ApplyJsonContract(options.SerializerOptions));

        // Malformed or incomplete bodies surface as BadHttpRequestException, so
        // they get the same {"error": ...} body as every other failure instead
        // of an empty 400.
        builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        builder.Services.AddExceptionHandler<ErrorResponseExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddOpenApi(options =>
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = serviceTitle;
                document.Info.Version = "v1";
                return Task.CompletedTask;
            }));

        return builder;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app, string serviceTitle)
    {
        app.UseExceptionHandler();

        app.MapOpenApi();
        app.MapScalarApiReference(options => options.WithTitle(serviceTitle));

        return app;
    }

    /// <summary>
    /// Binds Kestrel to the <c>PORT</c> variable that docker-compose.yml and the
    /// Kubernetes manifests set. It is applied as the HTTP_PORTS host setting,
    /// which overrides the base image's ASPNETCORE_HTTP_PORTS=8080 without the
    /// "overriding addresses" warning that UseUrls would log. When PORT is unset
    /// (a local run) the launch profile's applicationUrl is used instead.
    /// </summary>
    public static WebApplicationBuilder UseServicePort(this WebApplicationBuilder builder)
    {
        var port = builder.Configuration[PortVariable];
        if (string.IsNullOrWhiteSpace(port))
        {
            return builder;
        }

        if (!int.TryParse(port, out var number) || number is < 1 or > 65535)
        {
            throw new InvalidOperationException($"{PortVariable} must be a TCP port number, got \"{port}\".");
        }

        builder.WebHost.UseSetting(WebHostDefaults.HttpPortsKey, port);
        return builder;
    }

    /// <summary>
    /// Discovers the presentation modules of the service and registers them, so
    /// adding a resource group means adding a class, not editing Program.cs.
    /// </summary>
    public static IServiceCollection AddEndpointModules(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes()
                     .Where(t => t is { IsAbstract: false, IsInterface: false }
                                 && typeof(IEndpointModule).IsAssignableFrom(t)))
        {
            services.AddSingleton(typeof(IEndpointModule), type);
        }

        return services;
    }

    /// <summary>Maps every registered presentation module.</summary>
    public static WebApplication MapEndpointModules(this WebApplication app)
    {
        foreach (var module in app.Services.GetServices<IEndpointModule>())
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    /// <summary>
    /// snake_case JSON with string enums — the contract of api-specification.yaml,
    /// used for the HTTP API and for the Kafka message values alike.
    ///
    /// Required fields are enforced by the serializer itself: a request
    /// constructor parameter without a default must be present, and a
    /// non-nullable one must not be null, so the contracts declare what is
    /// required and no hand-written validator can drift from them.
    /// </summary>
    public static void ApplyJsonContract(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.PropertyNameCaseInsensitive = true;
        options.RespectNullableAnnotations = true;
        options.RespectRequiredConstructorParameters = true;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
    }
}
