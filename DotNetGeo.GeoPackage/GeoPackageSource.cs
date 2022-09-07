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

    public async Task<ExtendedFeatureCollection> GetFeatures(SearchRequest request)
    {
        return await Reader.GetFeatures(request);
    }

    public async Task<ExtendedFeature> GetFeature(string id)
    {
        return await Reader.GetFeature(Collection.Title, id, "EPSG:4326");
    }
}
