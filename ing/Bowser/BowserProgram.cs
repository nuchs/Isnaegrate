using Bowser.Data;
using Mewtils;

using static Epoxy.Grpc.Writer;
using static Jaundicedsage.Grpc.UserDir;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddGrpcClient<UserDirClient>("JaundicedSage", o =>
    {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:JaundicedSage"]);
    });
    builder.Services.AddGrpcClient<WriterClient>("Epoxy", o =>
    {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:Epoxy"]);
    });

    builder.Services.AddHostedService<UserRepoWorker>();
    builder.Services.AddSingleton<UserRepo>();
    builder.Services.AddSingleton<SessionManager>();
};

app.ConfigureRequestPipeline = pipeline =>
{
    if (!pipeline.Environment.IsDevelopment())
    {
        pipeline.UseExceptionHandler("/Error");
    }

    pipeline.UseStaticFiles();
    pipeline.UseRouting();
    pipeline.MapBlazorHub();
    pipeline.MapFallbackToPage("/_Host");
};

return app.Run();
