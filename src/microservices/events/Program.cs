using System.Reflection;
using CinemaAbyss.Events.Application;
using CinemaAbyss.Events.Infrastructure;
using CinemaAbyss.Events.Infrastructure.Hosting;

const string ServiceTitle = "Events Service";

var builder = WebApplication.CreateBuilder(args);

// Composition root: the only place that knows every layer.
builder.AddServiceDefaults(ServiceTitle);

builder.Services.AddEventsApplication();
builder.Services.AddEventsInfrastructure(builder.Configuration);
builder.Services.AddEndpointModules(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapServiceDefaults(ServiceTitle);
app.MapEndpointModules();

app.Run();
