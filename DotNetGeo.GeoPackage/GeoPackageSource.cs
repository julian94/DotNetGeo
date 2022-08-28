using DotNetGeo.Core;

namespace DotNetGeo.GeoPackage;
public class GeoPackageCollection : IGeoCollection
{
    private string FilePath;

    public string ID { get; init; }

    public GeoPackageCollection(string dbFile)
    {
        FilePath = dbFile;
    }

    // Get metadata about this collection

    // Get n features in bbox with p offset

    // Get specific feature
}
