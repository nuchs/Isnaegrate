using Mum.Data;
using Mum.Pages.Index;
using System.Reflection;
using static Epoxy.Grpc.Writer;
using static Resin.Grpc.Reader;

const string AppName = "Mum";
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

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddGrpcClient<WriterClient>("Epoxy", o => {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:Epoxy"]);
    });
    builder.Services.AddGrpcClient<ReaderClient>("Resin", o => {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:Resin"]);
    });

    builder.Services.AddSingleton<AccountRepo>();
    builder.Services.AddTransient<IndexViewModel>();

    return builder.Build();
}

void ConfigureRequestPipeline(WebApplication app)
{
    log.LogInformation("Configuring the request pipeline");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}

void StartApp(WebApplication app)
{
    log.LogInformation($"{AppName} is cocked, locked and ready to rock!");
    app.Run();
    log.LogInformation($"Bye from {AppName}");
}
