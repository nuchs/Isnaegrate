using Grpc.Net.Client;
using Epoxy.Grpc;
using static Epoxy.Grpc.PropositionExtensions;
using static Epoxy.Grpc.Writer;

namespace Nimm2;

internal class WriteTest : ITest
{
    private const string EventType = "TestType";
    private const string EventStream = "TestStream";

    public async Task Run()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5296");

        var client = new WriterClient(channel);
        var rand = new Random();

        var propSet = NewPropositionSet(
            EventStream,
            NewProposition(Guid.NewGuid(), EventType, "Nimm2", new DTO { Name = "Alice", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType, "Nimm2", new DTO { Name = "Bob", Guessed = rand.Next()}),
            NewProposition(Guid.NewGuid(), EventType, "Nimm2", new DTO { Name = "Charles", Guessed = rand.Next()})
        );

        Console.WriteLine($"Writing to {propSet.Stream}");
        foreach (var prop in propSet.Propositions)
        {
            Console.WriteLine($"\t{prop.Id} => {prop.Payload}");
        }

        await client.AppendAsync(propSet);
    }

    public class DTO
    {
        public string Name { get; set; } = "The man with no name";

        public int Guessed { get; set; }
    }
}
