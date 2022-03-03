using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace Epoxy.Grpc;

public static class EpoxyHelpers
{
    public static IsgEvent NewIsgEvent(string source, EventType type, byte[]? payload = null)
    {
        return new IsgEvent()
        {
            Id = new Uid() { Value = Guid.NewGuid().ToString() },
            Type = type,
            Source = source,
            When = Timestamp.FromDateTime(DateTime.UtcNow),
            Payload = payload is null ? ByteString.Empty : ByteString.CopyFrom(payload)
        };
    }

    public static string PrettyToString(this IsgEvent value)
    {
        var sb = new StringBuilder();

        sb.AppendLine(nameof(IsgEvent));
        sb.AppendLine($"Id     : {value.Id}");
        sb.AppendLine($"Type   : {value.Type}");
        sb.AppendLine($"Source : {value.Source}");
        sb.AppendLine($"When   : {value.When}");

        return sb.ToString();
    }
}
