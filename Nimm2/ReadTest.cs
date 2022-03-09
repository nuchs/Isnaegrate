using Grpc.Core;
using Grpc.Net.Client;
using Ing.Grpc.Common.Events;
using Ing.Grpc.Resin;
using System.Text;

namespace Nimm2;

internal class ReadTest : ITest
{
    public ReadTest()
    {

    }
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
        using var channel = GrpcChannel.ForAddress("http://localhost:5158");

        var client = new Reader.ReaderClient(channel);
        var reply = client.Read(new ReadRequest { Stream = EventStream.TestStream, Position = 0 });

        Console.WriteLine("Reading events");
        Console.WriteLine("--------------");
        await foreach (var isgEvent in reply.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine(ToPrettyString(isgEvent));
            Console.WriteLine();
        }
    }
}
