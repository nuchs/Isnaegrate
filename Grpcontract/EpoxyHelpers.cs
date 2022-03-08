using Ing.Grpc.Epoxy;
using Ing.Grpc.Common.Events;
using System.Text.Json;

namespace Epoxy.Grpc;

public static class EpoxyHelpers
{
    public static Proposition NewProposition<T>(Guid id, EventType type, string source, T payload)
        => new Proposition()
        {
            Id = id.ToIsgId(),
            Type = type,
            Source = source.Serialise(),
            Payload = payload.Serialise()
        };

    public static PropositionSet NewPropositionSet(EventStream stream, IEnumerable<Proposition> props)
    {
        var propSet = new PropositionSet();

        propSet.Propositions.AddRange(props);
        propSet.Stream = stream;

        return propSet;
    }

    private static IsgId ToIsgId(this Guid value) => new IsgId() { Value = value.ToString() };

    private static string Serialise<T>(this T value) => JsonSerializer.Serialize(value);
}
