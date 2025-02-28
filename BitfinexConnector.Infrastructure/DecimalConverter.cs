using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitfinexConnector.Infrastructure;

public class DecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => decimal.TryParse(reader.GetString(), out var num)
                ? num
                : throw new JsonException(),
            _ => throw new JsonException()
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal value,
        JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}