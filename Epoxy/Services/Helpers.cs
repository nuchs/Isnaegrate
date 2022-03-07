using Epoxy.Grpc.Reader;
using Epoxy.Grpc.Shared;
using Epoxy.Grpc.Streams;
using Epoxy.Grpc.Types;
using Epoxy.Grpc.Writer;
using EventStore.Client;
using Google.Protobuf.WellKnownTypes;
using System.Text;

namespace Epoxy.Services;

public static class Helpers
{
    public static EventData ToEventData(this Proposition prop)
        => new EventData(
            Uuid.Parse(prop.Id.Value),
            prop.Type.ToString(),
            Encoding.UTF8.GetBytes(prop.Payload),
            Encoding.UTF8.GetBytes(prop.Source));
}
