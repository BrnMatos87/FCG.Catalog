using FCG.Catalog.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

var runningInContainer =
    string.Equals(
        Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
        "true",
        StringComparison.OrdinalIgnoreCase);

if (builder.Environment.IsDevelopment() && !runningInContainer)
{
    builder.Configuration.AddJsonFile(
        "appsettings.Local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddWorkerServices(builder.Configuration);

var host = builder.Build();

await host.RunAsync();