using Epoxy.Grpc.Reader;
using Epoxy.Grpc.Shared;
using Epoxy.Grpc.Streams;
using Epoxy.Grpc.Types;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace Resin.Services;

public static class Helpers
{
    public static IsgEventSet NewIsgEventSet(string stream, IEnumerable<IsgEvent> events)
    {
        var set = new IsgEventSet();

        set.Events.AddRange(events);
        set.Stream = System.Enum.Parse<EventStream>(stream);

        return set;
    }

    public static IsgEvent NewIsgEvent(string id, string type, byte[] source, ulong position, DateTime when, byte[] payload)
        => new IsgEvent()
        {
            Id = new IsgId() { Value = id },
            Type = System.Enum.Parse<EventType>(type),
            Source = Encoding.UTF8.GetString(source),
            Position = position,
            When = Timestamp.FromDateTime(when),
            Payload = Encoding.UTF8.GetString(payload)
        };
}
