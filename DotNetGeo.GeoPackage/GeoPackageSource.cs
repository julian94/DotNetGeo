using DotNetGeo.Core;
using NetTopologySuite.Features;

namespace DotNetGeo.GeoPackage;

public class GeoPackageSource : IFeatureSource
{
    private string FilePath;

    public string ID { get; init; }
    public string Title { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string Description { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public Extent Extent { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

    public GeoPackageSource(string dbFile)
    {
        FilePath = dbFile;
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        throw new NotImplementedException();
    }

    public IFeature GetFeature(string id)
    {
        throw new NotImplementedException();
    }

    // Get metadata about this collection

    // Get n features in bbox with p offset

    // Get specific feature
}
