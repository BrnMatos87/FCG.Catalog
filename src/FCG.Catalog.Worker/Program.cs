using FCG.Catalog.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddWorkerServices(builder.Configuration);

var host = builder.Build();

await host.RunAsync();