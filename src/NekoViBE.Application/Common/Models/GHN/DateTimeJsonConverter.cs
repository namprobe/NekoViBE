using System.Text.Json;
using System.Text.Json.Serialization;

namespace NekoViBE.Application.Common.Models.GHN;

public class DateTimeJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;
            
            if (DateTime.TryParse(value, out var date))
                return date;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var seconds))
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        else
            writer.WriteNullValue();
    }
}

