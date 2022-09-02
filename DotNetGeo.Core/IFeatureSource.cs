using NetTopologySuite.Features;

namespace DotNetGeo.Core;

public interface IFeatureSource
{
    public Collection Collection { get; init; }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request);
    public IFeature GetFeature(string id);
}
