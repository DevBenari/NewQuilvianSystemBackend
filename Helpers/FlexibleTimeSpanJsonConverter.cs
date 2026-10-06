using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackend.Helpers;

/// <summary>
/// Converter fleksibel untuk System.Nullable&lt;TimeSpan&gt;.
/// Mampu membaca:
/// 1. null
/// 2. string ISO / waktu ("hh:mm:ss", "hh:mm")
/// 3. objek ticks dari client / frontend ({"ticks": 0}, {"ticks": 123456789})
/// 4. angka ticks (number)
/// Jika ticks &lt;= 0 atau string kosong, dikembalikan sebagai null.
/// </summary>
public class FlexibleNullableTimeSpanConverter : JsonConverter<TimeSpan?>
{
    public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt.TimeOfDay;
            }

            return null;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            long? ticks = null;
            int? hours = null;
            int? minutes = null;
            int? seconds = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propName = reader.GetString();
                    reader.Read();

                    if (string.Equals(propName, "ticks", StringComparison.OrdinalIgnoreCase))
                    {
                        if (reader.TokenType == JsonTokenType.Number)
                        {
                            ticks = reader.GetInt64();
                        }
                    }
                    else if (string.Equals(propName, "hours", StringComparison.OrdinalIgnoreCase))
                    {
                        if (reader.TokenType == JsonTokenType.Number) hours = reader.GetInt32();
                    }
                    else if (string.Equals(propName, "minutes", StringComparison.OrdinalIgnoreCase))
                    {
                        if (reader.TokenType == JsonTokenType.Number) minutes = reader.GetInt32();
                    }
                    else if (string.Equals(propName, "seconds", StringComparison.OrdinalIgnoreCase))
                    {
                        if (reader.TokenType == JsonTokenType.Number) seconds = reader.GetInt32();
                    }
                }
            }

            if (ticks.HasValue)
            {
                if (ticks.Value <= 0)
                {
                    return null;
                }

                return TimeSpan.FromTicks(ticks.Value);
            }

            if (hours.HasValue || minutes.HasValue || seconds.HasValue)
            {
                return new TimeSpan(hours ?? 0, minutes ?? 0, seconds ?? 0);
            }

            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var num = reader.GetInt64();
            if (num <= 0)
            {
                return null;
            }

            return TimeSpan.FromTicks(num);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(@"hh\:mm\:ss"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// Converter fleksibel untuk non-nullable TimeSpan.
/// </summary>
public class FlexibleTimeSpanConverter : JsonConverter<TimeSpan>
{
    private static readonly FlexibleNullableTimeSpanConverter Inner = new();

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = Inner.Read(ref reader, typeof(TimeSpan?), options);
        return result ?? TimeSpan.Zero;
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}
