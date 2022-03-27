using Google.Protobuf.WellKnownTypes;
using Resin.Grpc;
using System.Text;

namespace Resin.Services;

public static class Helpers
{
    public static IsgEvent NewIsgEvent(string id, string type, ReadOnlyMemory<byte> source, ulong position, DateTime when, ReadOnlyMemory<byte> payload)
        => new IsgEvent()
        {
            Id = id,
            Type = type,
            Source = Encoding.UTF8.GetString(source.ToArray()),
            Position = position,
            When = Timestamp.FromDateTime(when),
            Payload = Encoding.UTF8.GetString(payload.ToArray())
        };
}
