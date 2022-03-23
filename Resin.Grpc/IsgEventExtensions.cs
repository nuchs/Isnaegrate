using System.Text.Json;

namespace Resin.Grpc;

public static class IsgEventExtensions
{
    public static T? DeserialiseEvent<T>(this IsgEvent value)
        => JsonSerializer.Deserialize<T>(value.Payload);

    public static T GetEventType<T>(this IsgEvent value) where T : struct 
        => Enum.TryParse<T>(value.Type, out var type) ? type : default;

    public static Guid GetId(this IsgEvent value) 
        => Guid.TryParse(value.Id, out var id) ? id : Guid.Empty;
}
