using Google.Protobuf.WellKnownTypes;
using Ing.Grpc.Common.Events;
using Ing.Grpc.Resin;
using System.Text;

namespace Resin.Services;

public static class Helpers
{
    public static IsgEvent NewIsgEvent(string id, string type, ReadOnlyMemory<byte> source, ulong position, DateTime when, ReadOnlyMemory<byte> payload)
        => new IsgEvent()
        {
            Id = new IsgId() { Value = id },
            Type = System.Enum.Parse<EventType>(type),
            Source = Encoding.UTF8.GetString(source.ToArray()),
            Position = position,
            When = Timestamp.FromDateTime(when),
            Payload = Encoding.UTF8.GetString(payload.ToArray())
        };
}
