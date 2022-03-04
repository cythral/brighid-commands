using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brighid.Commands
{
    /// <summary>
    /// Converts between big integer and string.
    /// </summary>
    public class BigIntegerJsonConverter : JsonConverter<BigInteger>
    {
        /// <inheritdoc />
        public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return BigInteger.Parse(value ?? "0");
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
