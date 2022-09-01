using DotNetGeo.Core;

namespace DotNetGeo.GeoPackage;

public class GeoPackageSource : IFeatureSource
{
    private string FilePath;

    public string ID { get; init; }

    public GeoPackageSource(string dbFile)
    {
        FilePath = dbFile;
    }

    // Get metadata about this collection

    // Get n features in bbox with p offset

    // Get specific feature
}
