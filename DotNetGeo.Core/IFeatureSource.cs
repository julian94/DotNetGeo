namespace DotNetGeo.Core;

public interface IFeatureSource
{
    public string ID { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public object Extent { get; init; }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request);
    public IFeature GetFeature(string id);
}
