using Resin.Services;
using Mewtils;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddEventStoreClient(builder.Configuration["ConnectionStrings:EventStore"]);
    builder.Services.AddGrpc();
};

app.ConfigureRequestPipeline = pipeline =>
{
    pipeline.MapGrpcService<ReaderService>();
    pipeline.MapGet("/", () => "Only grpc connections accepted");
};

return app.Run();
