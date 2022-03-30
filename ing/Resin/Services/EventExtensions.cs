using EventStore.Client;
using Google.Protobuf.WellKnownTypes;
using Resin.Grpc;
using System.Text;

namespace Resin.Services;

public static class EventExtensions
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

    public static IsgEvent ToIsgEvent(this EventRecord ev)
      => new() { 
          Id = ev.EventId.ToString(),
          Type = ev.EventType,
          Source = Encoding.UTF8.GetString(ev.Metadata.ToArray()),
          Position = ev.EventNumber,
          When = Timestamp.FromDateTime(ev.Created),
          Payload = Encoding.UTF8.GetString(ev.Data.ToArray())
        };

    public static string Describe(this ulong? target)
        => $"position " + (target.HasValue ? target.Value.ToString() : "start");

    public static string Describe(this StreamPosition? target)
        => $"position " + (target.HasValue ? target.Value.ToString() : "start");
}
