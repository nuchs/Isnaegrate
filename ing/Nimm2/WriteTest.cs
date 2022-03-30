using Grpc.Net.Client;
using Epoxy.Grpc;
using static Epoxy.Grpc.PropositionExtensions;
using static Epoxy.Grpc.Writer;

namespace Nimm2;

internal class WriteTest : ITest
{
    private const string EventType = "TestType";
    private readonly string EventStream = "TestStream";
    private readonly int NumEvents = 3;

    public WriteTest(string[] args)
    {
        if (args.Length > 1)
        {
            EventStream = args[1];
        }

        if (args.Length > 2)
        {
            NumEvents = int.Parse(args[2]);
        }
    }

    public async Task Run()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:5296");

        var client = new WriterClient(channel);
        var rand = new Random();
        List<Proposition> props = new();

        for (int i = 0; i < NumEvents; i++)
        {
            props.Add(NewProposition(Guid.NewGuid(), EventType, "Nimm2", new DTO { Name = "Arthur", Guessed = i }));
        }

        var propSet = NewPropositionSet(
            EventStream,
            props.ToArray()
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
