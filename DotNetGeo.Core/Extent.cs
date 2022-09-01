namespace DotNetGeo.Core;

public class Extent
{
    [JsonPropertyName("spatial")]
    public SpatialExtent? Spatial { get; set; }

    [JsonPropertyName("temporal")]
    public TemporalExtent? Temporal { get; set; }
}
