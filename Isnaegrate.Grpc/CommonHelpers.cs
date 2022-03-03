using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Isnaegrate.Grpc.Events;

namespace Isnaegrate.Grpc;

public static class CommonHelpers
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
}
