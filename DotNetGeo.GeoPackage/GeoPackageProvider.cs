using DotNetGeo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetGeo.GeoPackage;

public class GeoPackageProvider
{
    public static async Task<IList<IFeatureSource>> GetSourcesFromDB(string dbFile)
    {
        var reader = await GeoPackageReader.MakeReader(dbFile);

        var collections = await reader.GetCollectionsFromMetadata();

        var sources = new List<IFeatureSource>();

        foreach (var collection in collections)
        {
            sources.Add(new GeoPackageSource(collection, reader));
        }

        return sources;
    }
}
