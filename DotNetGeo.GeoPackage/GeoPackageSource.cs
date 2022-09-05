using DotNetGeo.Core;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SQLitePCL;

namespace DotNetGeo.GeoPackage;

public class GeoPackageSource : IFeatureSource
{
    private GeoPackageReader Reader { get; init; }

    public Collection Collection { get; init; }

    internal GeoPackageSource(Collection collection, GeoPackageReader reader)
    {
        Collection = collection;
        Reader = reader;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        return Reader.GetFeatures(request);
    }

    public ExtendedFeature GetFeature(string id)
    {
        return Reader.GetFeature(Collection.Title, id, "EPSG:4326");
    }
}
