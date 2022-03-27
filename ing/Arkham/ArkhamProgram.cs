using Arkham.Ca;
using Mewtils;

var app = new StartUp(args);

app.AddServices = builder =>
{
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddSingleton<ICertCache, CertCache>();
    builder.Services.AddSingleton<IAuthority, Authority>();
};

app.ConfigureRequestPipeline = pipeline =>
{
    if (!pipeline.Environment.IsDevelopment())
    {
        pipeline.UseExceptionHandler("/Error");
    }

    pipeline.UseRouting();
    pipeline.MapBlazorHub();
    pipeline.MapFallbackToPage("/_Host");
};

return app.Run();