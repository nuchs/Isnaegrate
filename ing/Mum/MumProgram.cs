using Mum.Data;
using Mum.Pages.Index;

using static Epoxy.Grpc.Writer;
using static Resin.Grpc.Reader;

using Mewtils;

var app = new StartUp(args);

app.AddServices = builder =>
{
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
