using Epoxy.Grpc.Reader;
using Epoxy.Grpc.Shared;
using Epoxy.Grpc.Types;
using Epoxy.Grpc.Writer;
using System.Text;
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

    public static string ToPrettyString(this IsgEvent value)
    {
        var sb = new StringBuilder();

        sb.AppendLine(nameof(IsgEvent));
        sb.AppendLine($"\tId      : {value.Id}");
        sb.AppendLine($"\tType    : {value.Type}");
        sb.AppendLine($"\tSource  : {value.Source}");
        sb.AppendLine($"\tWhen    : {value.When}");
        sb.AppendLine($"\tPayload : {value.Payload }");

        return sb.ToString();
    }

    private static IsgId ToIsgId(this Guid value) => new IsgId() { Value = value.ToString() };

    private static string Serialise<T>(this T value) => JsonSerializer.Serialize(value);
}
