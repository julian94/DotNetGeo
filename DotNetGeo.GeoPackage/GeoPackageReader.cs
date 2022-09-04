using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetGeo.Core;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SQLitePCL;


namespace DotNetGeo.GeoPackage;

internal class GeoPackageReader
{
    private SqliteConnection Connection { get; init; }

    public GeoPackageReader(string dbFile)
    {
        var connectionString = new SqliteConnectionStringBuilder($"Data Source={dbFile}")
        {
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        Connection = new(connectionString);
    }

    /* Things we need:
     * 
     * Get list of Spatial reference systems (gpkg_spatial_ref_sys).
     * Get table of tables (gpkg_contents) 
     * Get N matching features from feature table X in SRS Y
     * Get specific feature from table X in SRS Y
     * 
     */



    public ExtendedFeature GetFeature(string table, string id, string spatialReferenceSystem)
    {
        var commandText =
            @"SELECT AsGPB(Transform(GeomFromGPB(geom), ($destinationSRSnumber), ($boundingBox), ($originSRS), ($destinationSRS))" +
            "FROM ($table) WHERE id=($id)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("table", table);
        searchCommand.Parameters.AddWithValue("id", id);

        // The bounding box of the table, this makes the conversion more accurate.
        searchCommand.Parameters.AddWithValue("boundingBox", null);
        searchCommand.Parameters.AddWithValue("originSRS", id); // maybe replace by refernce to other table?
        searchCommand.Parameters.AddWithValue("destinationSRS", spatialReferenceSystem);
        var results = searchCommand.ExecuteReader();

        var geomColumn = results.GetOrdinal("geom");

        var features = new List<ExtendedFeature>();
        var geoPackageReader = new GeoPackageGeoReader();

        while (results.Read())
        {
            var geometryStream = results.GetStream(geomColumn);

            features.Add(new()
            {
                ID = id,
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = new()
                {
                    { "id", id },
                },
                Links = new(),
            });
        }

        return features.First();
    }
}
