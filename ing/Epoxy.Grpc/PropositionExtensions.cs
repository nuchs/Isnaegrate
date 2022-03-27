using System.Text.Json;

namespace Epoxy.Grpc;

public static class PropositionExtensions
{
    public static Proposition NewProposition<T>(Guid id, string type, string source, T payload)
        => new Proposition()
        {
            Id = id.ToString(),
            Type = type,
            Source = source.Serialise(),
            Payload = payload.Serialise()
        };

    public static PropositionSet NewPropositionSet(string stream, params Proposition[] props)
    {
        var propSet = new PropositionSet();

        propSet.Propositions.AddRange(props);
        propSet.Stream = stream;

        return propSet;
    }


    private static string Serialise<T>(this T value) => JsonSerializer.Serialize(value);
}
