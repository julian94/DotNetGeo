using NetTopologySuite.Features;

namespace DotNetGeo.Core;

public class DataCentral
{
    private IDictionary<string, IFeatureSource> Sources { get; init; }

    public CollectionGroup CollectionGroup()
    {
        var group = new CollectionGroup()
        {
            Collections = new(),
            Links = new(),
        };
        foreach (var source in Sources)
        {
            group.Collections.Add(source.Value.Collection);
        }
        return group;
    }
    public Collection Collection(string collectionId)
    {
        if (collectionId is null) throw new ArgumentNullException();
        if (!Sources.ContainsKey(collectionId)) throw new ArgumentException();

        return Sources[collectionId].Collection;
    }

    public DataCentral(IList<IFeatureSource> sources)
    {
        Sources = new Dictionary<string, IFeatureSource>();

        foreach (var source in sources)
        {
            Sources[source.Collection.ID] = source;
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
