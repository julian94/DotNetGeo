using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace DotNetGeo.Core;

public class ExtendedFeatureCollection
{
    [JsonPropertyName("type")]
    public const string Type = "FeatureCollection";

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; }

    [JsonPropertyName("numberMatched")]
    public int? NumberMatched { get; set; }

    [JsonPropertyName("numberReturned")]
    public int? NumberReturned { get; set; }

    [JsonPropertyName("features")]
    public IList<Feature> Features { get; set; }
}

/// <summary>
/// Converts FeatureCollection objects to their JSON representation.
/// </summary>
public class ExtendedFeatureCollectionConverter : JsonConverter<ExtendedFeatureCollection>
{
    public ExtendedFeatureCollectionConverter()
    {
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="options">The calling serializer.</param>
    /// <returns>
    /// The object value.
    /// </returns>
    public override ExtendedFeatureCollection Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
    {

        var fc = new ExtendedFeatureCollection();
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals("type"))
            {
                if (!reader.ValueTextEquals("FeatureCollection"))
                {
                    throw new JsonException("must be FeatureCollection");
                }

            }
            else if (reader.ValueTextEquals("features"))
            {
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    fc.Features.Add(JsonSerializer.Deserialize<Feature>(ref reader, options));

                }
            }
            else
            {
                reader.Skip();
            }
        }

        return fc;
    }

    public override void Write(Utf8JsonWriter writer, ExtendedFeatureCollection value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "FeatureCollection");

        writer.WriteStartArray("features");
        foreach (var feature in value.Features) JsonSerializer.Serialize(writer, feature, options);
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
