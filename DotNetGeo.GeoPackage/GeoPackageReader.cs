using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetGeo.Core;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SQLitePCL;


namespace DotNetGeo.GeoPackage;

internal class GeoPackageReader : IDisposable
{
    private SqliteConnection Connection { get; init; }

    public GeoPackageReader(string dbFile)
    {
        var connectionString = new SqliteConnectionStringBuilder($"Data Source={dbFile}")
        {
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        Connection = new(connectionString);
        Connection.Open();
        Connection.EnableExtensions(true);
        Connection.LoadExtension("./mod_spatialite.dll");
        Connection.EnableExtensions(false);
    }

    /* Things we need:
     * 
     * Get list of Spatial reference systems (gpkg_spatial_ref_sys).
     * Get table of tables (gpkg_contents) 
     * Get N matching features from feature table X in SRS Y
     * Get specific feature from table X in SRS Y
     * 
     * Make all of this ASYNC as it's IO.
     * 
     */

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {
        var bbox = request.bbox.AsPolygon();
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(bbox);


        // ST_GeomFromText('LINESTRING(minx miny, maxx miny, maxx maxy, minx maxy, minx miny)')
        var matchCountCommandText =
            $"SELECT COUNT(*) FROM {request.collectionID} " +
            "WHERE Intersects(geom, " +
            "Transform(Extent(GeomFromGPB(($boundingBox))), ($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS)))";
        var matchCountCommand = new SqliteCommand(matchCountCommandText, Connection);
        matchCountCommand.Parameters.AddWithValue("$table", request.collectionID);
        matchCountCommand.Parameters.AddWithValue("$bbox", bboxBytes);
        matchCountCommand.Parameters.AddWithValue("$originSRS", "EPSG:4326");
        matchCountCommand.Parameters.AddWithValue("$destinationSRSnumber", int.Parse(GetSRS(request.collectionID).Split(":")[1]));
        matchCountCommand.Parameters.AddWithValue("$destinationSRS", GetSRS(request.collectionID));

        var matchCountReader = matchCountCommand.ExecuteReader();
        matchCountReader.Read();
        var matches = matchCountReader.GetInt32(0);

        var commandText =
            $"SELECT * FROM {request.collectionID}" +
            "WHERE (($table).geom && " +
            "Transform(Extent(GeomFromGPB(($boundingBox))), ($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS)))" +
            "LIMIT ($limit) OFFSET ($offset)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        //searchCommand.Parameters.AddWithValue("$table", request.collectionID);
        searchCommand.Parameters.AddWithValue("$bbox", bboxBytes);
        searchCommand.Parameters.AddWithValue("$originSRS", "EPSG:4326");
        searchCommand.Parameters.AddWithValue("$destinationSRSnumber", int.Parse(GetSRS(request.collectionID).Split(":")[1]));
        searchCommand.Parameters.AddWithValue("$destinationSRS", GetSRS(request.collectionID));
        searchCommand.Parameters.AddWithValue("$limit", request.limit);
        searchCommand.Parameters.AddWithValue("$offset", request.offset);
        var results = searchCommand.ExecuteReader();


        var features = GetFeatures(results);

        var featureCollection = new ExtendedFeatureCollection()
        {
            Features = features,
            NumberMatched = matches,
            NumberReturned = features.Count,
            Links = new(),
        };

        return featureCollection;
    }

    public ExtendedFeature GetFeature(string table, string id, string spatialReferenceSystem)
    {
        var originSRS = GetSRS(table);
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(GetBoundingBox(table));


        var commandText =
            "SELECT AsGPB(Transform(" + 
            "GeomFromGPB(geom), ($destinationSRSnumber), GeomFromGPB(($boundingBox)), ($originSRS), ($destinationSRS)) " +
            "FROM ($table) WHERE id=($id)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("$table", table);
        searchCommand.Parameters.AddWithValue("$id", id);

        // The bounding box of the table, this makes the conversion more accurate.
        searchCommand.Parameters.AddWithValue("$boundingBox", bboxBytes);

        searchCommand.Parameters.AddWithValue("$originSRS", originSRS);
        searchCommand.Parameters.AddWithValue("$destinationSRSnumber", int.Parse(spatialReferenceSystem.Split(":")[1]));
        searchCommand.Parameters.AddWithValue("$destinationSRS", spatialReferenceSystem);

        var reader = searchCommand.ExecuteReader();

        var columns = reader.FieldCount;

        var geomColumn = reader.GetOrdinal("geom");

        var features = GetFeatures(reader);

        return features.First();
    }

    private List<ExtendedFeature> GetFeatures(SqliteDataReader reader)
    {
        var columns = reader.FieldCount;

        var geomColumn = reader.GetOrdinal("geom");

        var features = new List<ExtendedFeature>();
        var geoPackageReader = new GeoPackageGeoReader();

        while (reader.Read())
        {
            var geometryStream = reader.GetStream(geomColumn);

            var properties = new Dictionary<string, object>();

            for (var i = 0; i < columns; i++)
            {
                // We really don't want to add the geometry as a property.
                if (i != geomColumn)
                {
                    properties.Add(reader.GetName(i), reader.GetValue(i));
                }
            }

            features.Add(new()
            {
                ID = (string)properties["id"],
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = properties,
                Links = new(),
            });
        }

        return features;
    }

    private Geometry GetBoundingBox(string table)
    {
        var srs = GetSRS(table);

        var bboxCommandText = 
            "SELECT AsGPB(Transform(Extent(" + 
            "MakeLine(MakePoint(min_x, min_y), MakePoint(max_x, max_y))), " +
            "($destinationSRSnumber), NULL, ($originSRS), ($destinationSRS))) " +
            "FROM gpkg_contents WHERE table_name=($table)";

        var bboxCommand = new SqliteCommand(bboxCommandText, Connection);
        bboxCommand.Parameters.AddWithValue("$originSRS", srs);
        bboxCommand.Parameters.AddWithValue("$destinationSRSnumber", 4326);
        bboxCommand.Parameters.AddWithValue("$destinationSRS", "EPSG:4326");
        bboxCommand.Parameters.AddWithValue("$table", table);

        var reader = bboxCommand.ExecuteReader();
        reader.Read();
        var bboxStream = reader.GetStream(0);

        var geoPackageReader = new GeoPackageGeoReader();
        var bbox = geoPackageReader.Read(bboxStream);

        return bbox;
    }

    private string GetSRS(string table)
    {
        var srsCommandText = "SELECT srs_id FROM gpkg_contents WHERE table_name=($table)";
        var srsCommand = new SqliteCommand(srsCommandText, Connection);
        srsCommand.Parameters.AddWithValue("$table", table);

        var srsReader = srsCommand.ExecuteReader();
        srsReader.Read();
        var srsColumn = srsReader.GetOrdinal("srs_id");
        var srsNumber = srsReader.GetString(srsColumn);


        var orgCommandText = "SELECT organization FROM gpkg_spatial_ref_sys WHERE srs_id=($srs)";
        var orgCommand = new SqliteCommand(orgCommandText, Connection);
        orgCommand.Parameters.AddWithValue("$srs", srsNumber);

        var orgReader = orgCommand.ExecuteReader();
        orgReader.Read();
        var orgColumn = orgReader.GetOrdinal("organization");
        var orgName = orgReader.GetString(orgColumn);


        return $"{orgName}:{srsNumber}";
    }

    internal IList<Collection> GetCollectionsFromMetadata()
    {

        var commandText = "SELECT * FROM gpkg_contents";
        var command = new SqliteCommand(commandText, Connection);

        var reader = command.ExecuteReader();

        var tableNameColumn = reader.GetOrdinal("table_name");
        var dataTypeColumn = reader.GetOrdinal("data_type");
        var descriptionColumn = reader.GetOrdinal("description");

        var list = new List<Collection>();
        while (reader.Read())
        {
            if (!reader.GetString(dataTypeColumn).Equals("features")) continue;
            
            list.Add(new()
            {
                ID = reader.GetString(tableNameColumn),
                Title = reader.GetString(tableNameColumn),
                Description = reader.GetString(descriptionColumn),
                crs = new(),
                Extent = null,
                Links = new(),
            });
        }

        foreach(var collection in list)
        {
            collection.crs.Add("EPSG:4326 but fancy");
            collection.crs.Add("Whichever format it's originally in.");
            collection.Extent = new()
            {
                Spatial = new()
                {
                    BoundingBox = new()
                    {
                        BoundingBox.FromEnvelope(GetBoundingBox(collection.ID).EnvelopeInternal),
                    },
                },
                Temporal = null,
            };
        }

        return list;
    }

    public void Dispose()
    {
        Connection?.Dispose();
    }
}
