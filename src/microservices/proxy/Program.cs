using System.Reflection;
using CinemaAbyss.Proxy.Domain.Policies;
using CinemaAbyss.Proxy.Infrastructure;
using CinemaAbyss.Proxy.Infrastructure.Configuration;
using CinemaAbyss.Proxy.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Composition root: the only place that knows every layer.
builder.AddServiceDefaults();
builder.Services.AddProxyInfrastructure(builder.Configuration);
builder.Services.AddEndpointModules(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapEndpointModules();
app.MapReverseProxy();

var upstreams = app.Services.GetRequiredService<UpstreamOptions>();
app.Logger.LogInformation(
    "Strangler Fig proxy: monolith {Monolith}, movies-service {Movies}, events-service {Events}; movies routing: {Policy}",
    upstreams.MonolithUrl,
    upstreams.MoviesServiceUrl,
    upstreams.EventsServiceUrl,
    app.Services.GetRequiredService<MoviesMigrationPolicy>());

app.Run();
