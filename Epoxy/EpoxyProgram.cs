using Epoxy.Services;
using EventStore.Client;
using System.Reflection;

const string AppName = "Epoxy";
var log = CreateBootLogger();

try
{
    var app = BuildApp();

    ConfigureRequestPipeline(app);
    StartApp(app);

    return 0;
}
catch (Exception ex)
{
    log.LogError(ex, $"An error occured; {AppName} was terminated");

    return 1;
}

/* -------------------- Helpers -------------------- */

ILogger<Program> CreateBootLogger()
{
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Hello!");
    logger.LogInformation($"{AppName} : v{Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}");

    return logger;
}

WebApplication BuildApp()
{
    log.LogInformation("Building Application");

    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddEventStoreClient(builder.Configuration["ConnectionStrings:EventStore"]);
    builder.Services.AddGrpc();

    return builder.Build();
}

void ConfigureRequestPipeline(WebApplication app)
{
    log.LogInformation("Configuring the request pipeline");
    app.MapGrpcService<GreeterService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
}

void StartApp(WebApplication app)
{
    log.LogInformation($"{AppName} is cocked, locked and ready to rock!");
    app.Run();
    log.LogInformation($"Bye from {AppName}");
}
