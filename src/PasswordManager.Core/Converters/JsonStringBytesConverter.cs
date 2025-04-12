using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PasswordManager.Core.Converters;

/// <summary>
/// JSON converter for byte arrays
/// </summary>
public class JsonStringBytesConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            return Convert.FromHexString(reader.GetString());
        }
        catch (FormatException e)
        {
            throw new JsonException("Wrong HEX string", e);
        }
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}
