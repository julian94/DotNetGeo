using NetTopologySuite.Features;

namespace DotNetGeo.Core;

public interface IFeatureSource
{
    public Collection Collection { get; init; }

    public Task<ExtendedFeatureCollection> GetFeatures(SearchRequest request);
    public Task<ExtendedFeature> GetFeature(string id);
}
