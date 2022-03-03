using Grpc.Net.Client;
using Isnaegrate.Grpc.Events;
using Resin.Grpc;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("http://localhost:5296");
var client = new Reader.ReaderClient(channel);
var reply = await client.ReadAsync(
                  new Request { Stream = EventType.Test, Position = -1 });
Console.WriteLine("Read event");
Console.WriteLine("------------");
Console.WriteLine($"Id     : {reply.Id}");
Console.WriteLine($"Type   : {reply.Type}");
Console.WriteLine($"Source : {reply.Source}");
Console.WriteLine($"When   : {reply.When}");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();