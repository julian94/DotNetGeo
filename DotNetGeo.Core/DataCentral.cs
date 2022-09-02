using NetTopologySuite.Features;

namespace DotNetGeo.Core;

public class DataCentral
{
    private IDictionary<string, IFeatureSource> Sources { get; init; }

    public CollectionGroup CollectionGroup { get; init; }

    public DataCentral(IList<IFeatureSource> sources)
    {
        Sources = new Dictionary<string, IFeatureSource>();
        CollectionGroup = new()
        {
            Collections = new(),
            Links = new(),
        };

        foreach (var source in sources)
        {
            Sources[source.Collection.ID] = source;
            CollectionGroup.Collections.Add(source.Collection);
        }


    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        return Sources[request.collectionID].GetFeatures(request);
    }

    public ExtendedFeature GetFeature(string collection, string id)
    {
        return Sources[collection].GetFeature(id);
    }
}
