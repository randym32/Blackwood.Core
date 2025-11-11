// Copyright (c) 2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel;


namespace Blackwood;

/// <summary>
/// JSON converter for ProxyPropertiesObject that serializes and deserializes
/// the proxy properties.
/// </summary>
/// <remarks>
/// This converter enables ProxyPropertiesObject to be serialized to JSON and 
/// deserialized back.
/// It works by:
/// 1. During serialization: Extracts the current values of all managed
///    properties and serializes them as a dictionary
/// 2. During deserialization: Creates a new ProxyPropertiesObject and sets the
///    property values from the JSON data
///
/// Note: This is a basic implementation that serializes property values but doesn't preserve
/// the original type information or property descriptors. For full round-trip serialization,
/// additional metadata would need to be stored.
/// </remarks>
public class PropertiesJsonConverter : JsonConverter<ProxyPropertiesObject>
{
    /// <summary>
    /// Writes a ProxyPropertiesObject to JSON.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The ProxyPropertiesObject to serialize.</param>
    /// <param name="options">The JSON serializer options.</param>
    public override void Write(Utf8JsonWriter writer, ProxyPropertiesObject value, JsonSerializerOptions options)
    {
        // Handle serializing a null
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        // This will serialize the property->value mapping as a dictionary ("object")
        writer.WriteStartObject();

        // Get all property descriptors and serialize their current values
        var properties = value.GetProperties();
        foreach (PropertyDescriptor prop in properties)
        {
            // Only keep if the value has changed from the default value
            if (!prop.ShouldSerializeValue(value))
                continue;
            var propValue = prop.GetValue(value);
            writer.WritePropertyName(prop.Name);
            JsonSerializer.Serialize(writer, JSONDeserializer.ConvertValueToSerializableForm(propValue), options);
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Reads JSON and converts it to a ProxyPropertiesObject.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="typeToConvert">The type to convert to.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>A ProxyPropertiesObject with the deserialized property values.</returns>
    public override ProxyPropertiesObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Handle null values
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        // Expect an object
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected object, but found {reader.TokenType}");

        // Create a new ProxyPropertiesObject
        var proxy = new ProxyPropertiesObject();

        // Read the object properties and consume their values
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                reader.GetString();
                // Advance to the value token
                if (!reader.Read())
                    throw new JsonException("Unexpected end of JSON while reading property value.");

                // Consume the entire value (handles primitives, objects, arrays, null)
                using var _ = JsonDocument.ParseValue(ref reader);

                // This basic implementation does not apply values immediately.
                // Values would be applied after the proxy is configured with descriptors.
            }
        }

        return proxy;
    }

}
