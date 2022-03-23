using Grpc.Core;
using Grpc.Net.Client;
using Resin.Grpc;
using System.Text;

namespace Nimm2;

internal class SubscribeTest : ITest
{
    private const string EventStream = "TestStream";

    public string ToPrettyString(IsgEvent value)
    {
        var sb = new StringBuilder();

        sb.AppendLine(nameof(IsgEvent));
        sb.AppendLine($"\tId      : {value.Id}");
        sb.AppendLine($"\tType    : {value.Type}");
        sb.AppendLine($"\tSource  : {value.Source}");
        sb.AppendLine($"\tWhen    : {value.When}");
        sb.AppendLine($"\tPayload : {value.Payload }");

        return sb.ToString();
    }

    public async Task Run()
    {
        try
        {
            await DoSub();

        }
        catch (Exception e)
        {
            Console.WriteLine("Proxy dropped connection");
            Console.WriteLine(e);
        }    
    }

    private async Task DoSub()
    {
        var channel = GrpcChannel.ForAddress("http://localhost:5158");

        var client = new Reader.ReaderClient(channel);
        var reply = client.Subscribe(new ReadRequest { Stream = EventStream, Position = 0 });

        Console.WriteLine("Subscribed to events");
        Console.WriteLine("--------------------");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"Recieved after {stopwatch.ElapsedMilliseconds}");
            Console.WriteLine(ToPrettyString(isgEvent));
            Console.WriteLine();
        }
        stopwatch.Stop();
        Console.WriteLine($"Stopping at {stopwatch.ElapsedMilliseconds}");
    }
}
