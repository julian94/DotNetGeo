using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace DotNetGeo.Core;

public class ExtendedFeature
{
    [JsonPropertyName("type")]
    public const string Type = "Feature";

    [JsonPropertyName("ID")]
    public string ID { get; set; }

    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }

    [JsonPropertyName("bbox")]
    public BoundingBox BoundingBox { get; set; }

    [JsonPropertyName("links")]
    public List<Link> Links { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; }

    public void PopulateLinks(string address, string collection, bool isItem)
    {
        Links.Add(new()
        {
            HRef = $"{address}/collections/{collection}",
            Rel = "collection",
            Type = "application/geo+json",
            Title = "this collection document",
        });
        Links.Add(new()
        {
            HRef = $"{address}/collections/{collection}/{ID}",
            Rel = "canonical",
            Title = "canonical URI of this item",
        });
        Links.Add(new()
        {
            HRef = $"{address}/collections/{collection}/{ID}.json",
            Rel = isItem ? "item" : "self",
            Type = "application/geo+json",
            Title = "this document",
        });

        /* This is only here for reference as we currently only support json.
        Links.Add(new()
        {
            HRef = $"{address}/collections/{collection}/{ID}.html",
            Rel = "alternate",
            Type = "text/html",
            Title = "this document as HTML",
        });
        */
    }
}
/*
/// <summary>
/// Converts FeatureCollection objects to their JSON representation.
/// </summary>
public class ExtendedFeatureConverter : JsonConverter<ExtendedFeature>
{
    public ExtendedFeatureConverter()
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
    public override ExtendedFeature Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
    {

        var f = new ExtendedFeature();
        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            if (reader.ValueTextEquals("type"))
            {
                if (!reader.ValueTextEquals("Feature"))
                {
                    throw new JsonException("Must be a Feature");
                }

            }
            else if (reader.ValueTextEquals("geometry"))
            {
                // Parse
            }
            else if (reader.ValueTextEquals("properties"))
            {
                // Parse
            }
            else if (reader.ValueTextEquals("bbox"))
            {
                // Parse
            }
            else
            {
                reader.Skip();
            }
        }

        return f;
    }

    public override void Write(Utf8JsonWriter writer, ExtendedFeature value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", "Feature");
        writer.WriteString("id", value.ID);

        writer.WritePropertyName("geometry");
        JsonSerializer.Serialize(writer, value.Geometry, options);

        if (value.Links != null && value.Links.Count > 0)
        {
            writer.WritePropertyName("links");
            writer.WriteStartArray();
            foreach (var link in value.Links) JsonSerializer.Serialize(writer, link, options);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
*/
