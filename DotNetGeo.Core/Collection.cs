namespace DotNetGeo.Core;

public class CollectionGroup
{
    [JsonPropertyName("collections")]
    public List<Collection>? Collections { get; set; }

    [JsonPropertyName("links")]
    public List<Link>? Links { get; set; }
    public void PopulateLinks(string address)
    {
        Links.Add(new()
        {
            HRef = $"{address}/collections",
            Rel = "self",
            Type = "application/json",
            Title = "All collections",
        });
        foreach (var collection in Collections) collection.PopulateLinks(address, true);
    }
}

public class Collection
{
    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("extent")]
    public Extent? Extent { get; set; }

    [JsonPropertyName("links")]
    public List<Link>? Links { get; set; }

    // Will typically just be: ["http://www.opengis.net/def/crs/OGC/1.3/CRS84"]
    [JsonPropertyName("crs")]
    public List<string> crs { get; set; }
    public void PopulateLinks(string address, bool isItem)
    {
        Links.Add(new()
        {
            HRef = $"{address}/collections/{ID}",
            Rel = isItem ? "item" : "self",
            Type = "application/json",
            Title = Title,
        });
        Links.Add(new()
        {
            HRef = $"{address}/collections/{ID}/items",
            Rel = "items",
            Type = "application/geo+json",
            Title = Title,
        });
    }
}
