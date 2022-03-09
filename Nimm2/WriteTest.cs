using Grpc.Net.Client;
using Ing.Grpc.Common.Events;
using Ing.Grpc.Epoxy;
using static Epoxy.Grpc.EpoxyHelpers;
using static Ing.Grpc.Epoxy.Writer;

namespace Nimm2;

internal class WriteTest : ITest
{
    public async Task Run()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5296");

        var client = new WriterClient(channel);
        var propSet = new PropositionSet();
        var rand = new Random();

        propSet.Propositions.AddRange(new[]
        {
            NewProposition(Guid.NewGuid(), EventType.TestType, "Nimm2", new DTO { Name = "Alice", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType.TestType, "Nimm2", new DTO { Name = "Bob", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType.TestType, "Nimm2", new DTO { Name = "Charles", Guessed = rand.Next()}),
        });
        propSet.Stream = EventStream.TestStream;

        Console.WriteLine($"Writing to {propSet.Stream}");
        foreach (var prop in propSet.Propositions)
        {
            Console.WriteLine($"\t{prop.Id.Value} => {prop.Payload}");
        }

        await client.AppendAsync(propSet);
    }

    public class DTO
    {
        public string Name { get; set; } = "The man with no name";

        public int Guessed { get; set; }
    }
}
