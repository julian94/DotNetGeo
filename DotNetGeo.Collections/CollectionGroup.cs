namespace DotNetGeo.Core;

public class CollectionGroup
{
    [JsonPropertyName("collections")]
    public List<IGeoCollection> Collections { get; init; }

    [JsonPropertyName("links")]
    public List<Link>? Links { get; init; }
}
