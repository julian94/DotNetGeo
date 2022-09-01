using NetTopologySuite.Features;

namespace DotNetGeo.Core;

public class DataCentral
{
    private IDictionary<string, IFeatureSource> Sources { get; init; }
    public DataCentral(IList<IFeatureSource> sources)
    {
        Sources = new Dictionary<string, IFeatureSource>();
        foreach (var source in sources)
        {
            Sources[source.ID] = source;
        }
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        return Sources[request.collectionID].GetFeatures(request);
    }

    public IFeature GetFeature(string collection, string id)
    {
        return Sources[collection].GetFeature(id);
    }
}
