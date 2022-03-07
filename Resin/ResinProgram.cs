using Epoxy.Grpc;
using Epoxy.Grpc.Streams;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5296");
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
Console.WriteLine("Press any key to exit...");
Console.ReadKey();