using EventStore.Client;
using Google.Protobuf.WellKnownTypes;
using Resin.Grpc;
using System.Text;

namespace Resin.Services;

internal static class UtilExtensions
{
    internal static IsgEvent ToIsgEvent(this EventRecord ev)
      => new() { 
          Id = ev.EventId.ToString(),
          Type = ev.EventType,
          Source = Encoding.UTF8.GetString(ev.Metadata.ToArray()),
          Position = ev.EventNumber,
          When = Timestamp.FromDateTime(ev.Created),
          Payload = Encoding.UTF8.GetString(ev.Data.ToArray())
        };

    internal static string Describe(this ulong? target)
        => $"position " + (target.HasValue ? target.Value : "start");

    internal static string Describe(this StreamPosition? target)
        => $"position " + (target.HasValue ? target.Value.ToString() : "start");
}
