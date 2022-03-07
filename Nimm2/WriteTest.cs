using Epoxy.Grpc.Streams;
using Epoxy.Grpc.Types;
using Epoxy.Grpc.Writer;
using Grpc.Net.Client;
using static Epoxy.Grpc.EpoxyHelpers;
using static Epoxy.Grpc.Writer.Writer;

namespace Nimm2;

internal class WriteTest : ITest
{
    public async Task Run(GrpcChannel channel)
    {
        var client = new WriterClient(channel);
        var propSet = new PropositionSet();
        var rand = new Random();

        propSet.Propositions.AddRange(new[]
        {
            NewProposition(Guid.NewGuid(), EventType.Test, "Nimm2", new DTO { Name = "Alice", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType.Test, "Nimm2", new DTO { Name = "Bob", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType.Test, "Nimm2", new DTO { Name = "Charles", Guessed = rand.Next()}),
        });
        propSet.Stream = EventStream.Test;

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
