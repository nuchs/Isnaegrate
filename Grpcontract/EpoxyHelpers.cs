using Epoxy.Grpc.Streams;
using Epoxy.Grpc.Types;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace Epoxy.Grpc;

public static class EpoxyHelpers
{
    public static IsgEvent NewIsgEvent(string id, string stream, string type, string source, ulong position, DateTime when, byte[]? payload = null)
    {
        return new IsgEvent()
        {
            Id = new Uid() { Id = id },
            Stream = System.Enum.Parse<EventStream>(stream),
            Type = System.Enum.Parse<EventType>(type),
            Source = source,
            Position = position,
            When = Timestamp.FromDateTime(when),
            Payload = payload is null ? ByteString.Empty : ByteString.CopyFrom(payload)
        };
    }

    public static string ToPrettyString(this IsgEvent value)
    {
        var sb = new StringBuilder();

        sb.AppendLine(nameof(IsgEvent));
        sb.AppendLine($"\tId     : {value.Id}");
        sb.AppendLine($"\tStream : {value.Stream}");
        sb.AppendLine($"\tType   : {value.Type}");
        sb.AppendLine($"\tSource : {value.Source}");
        sb.AppendLine($"\tWhen   : {value.When}");

        return sb.ToString();
    }
}
