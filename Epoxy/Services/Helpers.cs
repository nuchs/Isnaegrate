using EventStore.Client;
using Epoxy.Grpc;
using System.Text;

namespace Epoxy.Services;

public static class Helpers
{
    public static EventData ToEventData(this Proposition prop)
        => new EventData(
            Uuid.Parse(prop.Id),
            prop.Type.ToString(),
            Encoding.UTF8.GetBytes(prop.Payload),
            Encoding.UTF8.GetBytes(prop.Source));
}
