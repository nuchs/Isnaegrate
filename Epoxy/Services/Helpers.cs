using EventStore.Client;
using Ing.Grpc.Epoxy;
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
