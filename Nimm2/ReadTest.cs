using Epoxy.Grpc;
using Epoxy.Grpc.Reader;
using Epoxy.Grpc.Streams;
using Grpc.Net.Client;

namespace Nimm2;

internal class ReadTest : ITest
{
    public async Task Run(GrpcChannel channel)
    {
        var client = new Reader.ReaderClient(channel);
        var reply = await client.ReadAsync(
                          new ReadRequest { Stream = EventStream.Test, Position = 0 });

        Console.WriteLine("Reading events");
        Console.WriteLine("--------------");
        foreach (var isgEvent in reply.Events)
        {
            Console.WriteLine(isgEvent.ToPrettyString());
            Console.WriteLine();
        }
    }
}
