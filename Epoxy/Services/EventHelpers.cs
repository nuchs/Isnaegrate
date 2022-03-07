using Epoxy.Grpc.Reader;
using Epoxy.Grpc.Shared;
using Epoxy.Grpc.Streams;
using Epoxy.Grpc.Types;
using Epoxy.Grpc.Writer;
using EventStore.Client;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace Epoxy.Services;

public static class EventHelpers
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

    public static EventData ToEventData(this Proposition prop)
        => new EventData(
            Uuid.Parse(prop.Id.Value),
            prop.Type.ToString(),
            Encoding.UTF8.GetBytes(prop.Payload),
            Encoding.UTF8.GetBytes(prop.Source));
}
