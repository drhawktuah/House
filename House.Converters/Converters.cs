using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace House.House.Converters;

public sealed class SnowflakeJSONConverter : JsonConverter<ulong>
{
    public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();

            if (ulong.TryParse(value, out ulong ID))
            {
                return ID;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetUInt64();
        }

        return 0UL;
    }

    public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public sealed class AuditLogActionTypeConverter : JsonConverter<AuditLogActionType>
{
    public override AuditLogActionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return (AuditLogActionType)reader.GetUInt32();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string? value = reader.GetString();

            if (int.TryParse(value, out int v))
            {
                return (AuditLogActionType)v;
            }
        }

        return 0;
    }

    public override void Write(Utf8JsonWriter writer, AuditLogActionType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}