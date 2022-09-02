using DotNetGeo.Core;
using NetTopologySuite.Features;

namespace DotNetGeo.GeoPackage;

public class GeoPackageSource : IFeatureSource
{
    private string FilePath;

    public Collection Collection { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

    public GeoPackageSource(string dbFile)
    {
        FilePath = dbFile;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        throw new NotImplementedException();
    }

    public ExtendedFeature GetFeature(string id)
    {
        throw new NotImplementedException();
    }

    // Get metadata about this collection

    // Get n features in bbox with p offset

    // Get specific feature
}
