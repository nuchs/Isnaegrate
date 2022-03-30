using Grpc.Core;
using Resin.Grpc;
using static Resin.Grpc.Reader;

namespace Resin.Services;

public class ReaderService : ReaderBase
{
    private readonly ConnectionManager connectionMgr;

    public ReaderService(ConnectionManager connectionMgr)
    {
        this.connectionMgr = connectionMgr;
    }

    public override async Task Read(ReadRequest request, IServerStreamWriter<IsgEvent> responseStream, ServerCallContext context)
    {
        var id = WhoAmI(context);

        await foreach (var result in connectionMgr.Read(id, request.Stream, request.Position))
        {
            await responseStream.WriteAsync(result);
        }
    }

    public override async Task Subscribe(ReadRequest request, IServerStreamWriter<IsgEvent> responseStream, ServerCallContext context)
    {
        var id = WhoAmI(context);

        await using var subscription = connectionMgr.Subscribe(id, request.Stream, request.Position);
        await foreach (var result in subscription)
        {
            await responseStream.WriteAsync(result);
        }
    }

    private string WhoAmI(ServerCallContext context)
            => context.AuthContext.PeerIdentity.FirstOrDefault()?.Name ?? "No-one";
}
