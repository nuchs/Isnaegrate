using Epoxy.Services;
using System.Reflection;

string AppName = Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "The app with no name";
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
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddJsonConsole(conf =>
    {
        conf.IncludeScopes = false;
        conf.TimestampFormat = $"yyyy-MM-dd HH:mm:ss.fff";
        conf.UseUtcTimestamp = true;
    }));

    var logger = loggerFactory.CreateLogger<Program>();

    logger.LogInformation("Hello!");
    logger.LogInformation($"{AppName} : v{Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}");

    return logger;
}

WebApplication BuildApp()
{
    var builder = WebApplication.CreateBuilder(args);

    log.LogInformation("Building {} for {}", AppName, builder.Environment.EnvironmentName);
    
    builder.Services.AddEventStoreClient(builder.Configuration["ConnectionStrings:EventStore"]);
    builder.Services.AddGrpc();

    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole(conf =>
    {
        conf.IncludeScopes = false;
        conf.TimestampFormat = $"yyyy-MM-dd HH:mm:ss.fff";
        conf.UseUtcTimestamp = true;
    });

    return builder.Build();
}

void ConfigureRequestPipeline(WebApplication app)
{
    log.LogInformation("Configuring the request pipeline");
    app.MapGrpcService<WriterService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
}

void StartApp(WebApplication app)
{
    log.LogInformation($"{AppName} is cocked, locked and ready to rock!");
    app.Run();
    log.LogInformation($"Bye from {AppName}");
}
