using Arkham.Ca;
using System.Reflection;

const string AppName = "Arkham";
var log = CreateBootLogger();

try
{
    var app = BuildApp();
    ConfigureRequestPipeline(app);
    log.LogInformation($"{AppName} is cocked, locked and ready to rock!");
    app.Run();
    log.LogInformation($"Bye from {AppName}");

    return 0;
}
catch (Exception ex)
{
    log.LogError(ex, $"An error occured when starting {AppName}");
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
    var builder = WebApplication.CreateBuilder(args);

    log.LogInformation("Building {} for {}", AppName, builder.Environment.EnvironmentName);

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddSingleton<ICertCache, CertCache>();
    builder.Services.AddSingleton<IAuthority, Authority>();

    return builder.Build();
}

void ConfigureRequestPipeline(WebApplication app)
{
    log.LogInformation("Configuring the request pipeline");

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");
}
