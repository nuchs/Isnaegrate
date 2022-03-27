using JaundicedSage.Services;
using Mewtils;

using static Resin.Grpc.Reader;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddGrpc();
    builder.Services.AddGrpcClient<ReaderClient>("Resin", o => {
        o.Address = new Uri(builder.Configuration["ConnectionStrings:Resin"]);
    });

    builder.Services.AddHostedService<UserRepoWorker>();
    builder.Services.AddSingleton<UserRepo>();
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<UserDirService>();
    pipeline.MapGet("/", () => "Only grpc connections accepted");
};

return app.Run();
