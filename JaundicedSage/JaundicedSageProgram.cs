using JaundicedSage.Services;
using System.Reflection;
using static Resin.Grpc.Reader;

const string AppName = "JaundicedSage";
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
    using var loggerFactory = LoggerFactory.Create(builder => builder.AddSystemdConsole(conf =>
    {
        conf.IncludeScopes = true;
        conf.TimestampFormat = "H:mm:ss.fff K ";
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

    builder.Logging.ClearProviders();
    builder.Logging.AddSystemdConsole(conf =>
    {
        conf.IncludeScopes = false;
        conf.TimestampFormat = "H:mm:ss.fff K ";
        conf.UseUtcTimestamp = true;
    });

    builder.Services.AddGrpc();
    builder.Services.AddGrpcClient<ReaderClient>("Resin", o => {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:Resin"]);
    });
    builder.Services.AddSingleton<UserRepo>();

    return builder.Build();
}

void ConfigureRequestPipeline(WebApplication app)
{
    log.LogInformation("Configuring the request pipeline");
    app.MapGrpcService<UserDirService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
}

void StartApp(WebApplication app)
{
    log.LogInformation("{} is cocked, locked and ready to rock!", AppName);
    app.Run();
    log.LogInformation("Bye from {}", AppName);
}

