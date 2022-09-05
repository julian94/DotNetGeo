using DotNetGeo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetGeo.GeoPackage;

public class GeoPackageProvider
{
    public static IList<IFeatureSource> GetSourcesFromDB(string dbFile)
    {
        var reader = new GeoPackageReader(dbFile);

        var collections = reader.GetCollectionsFromMetadata();

        var sources = new List<IFeatureSource>();

        foreach (var collection in collections)
        {
            sources.Add(new GeoPackageSource(collection, reader));
        }

        return sources;
    }
}
