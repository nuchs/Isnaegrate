using Grpc.Net.Client;
using Epoxy.Grpc;

using var channel = GrpcChannel.ForAddress("http://localhost:5296");
var client = new Reader.ReaderClient(channel);
var reply = await client.ReadAsync(
                  new ReadRequest { Stream = EventType.Test, Position = -1 });
Console.WriteLine("Read event");
Console.WriteLine("------------");
Console.WriteLine(reply.PrettyToString());
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();