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

    private GeoPackageReader(SqliteConnection connection)
    {
        Connection = connection;
    }

    public static async Task<GeoPackageReader> MakeReader(string dbFile)
    {
        var connectionString = new SqliteConnectionStringBuilder($"Data Source={dbFile}")
        {
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        connection.EnableExtensions(true);
        connection.LoadExtension("./mod_spatialite.dll");
        connection.EnableExtensions(false);

        return new GeoPackageReader(connection);
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

    public async Task<ExtendedFeatureCollection> GetFeatures(SearchRequest request)
    {
        var bbox = request.bbox.AsPolygon();
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(bbox);

        var filterText = $"FROM {request.collectionID} WHERE Intersects(geom, ST_GeomFromText(($linestringtext)))";

        // ST_GeomFromText('LINESTRING(minx miny, maxx miny, maxx maxy, minx maxy, minx miny)')
        var matchCountCommandText = "SELECT COUNT(*)  " + filterText;
        var matchCountCommand = new SqliteCommand(matchCountCommandText, Connection);
        matchCountCommand.Parameters.AddWithValue("$table", request.collectionID);
        matchCountCommand.Parameters.AddWithValue("$linestringtext", $"LINESTRING({request.bbox.BottomLeft.X} {request.bbox.BottomLeft.Y}, {request.bbox.TopRight.X} {request.bbox.BottomLeft.Y}, {request.bbox.TopRight.X} {request.bbox.TopRight.Y}, {request.bbox.BottomLeft.X} {request.bbox.TopRight.Y}, {request.bbox.BottomLeft.X} {request.bbox.BottomLeft.Y})");
        matchCountCommand.Parameters.AddWithValue("$originSRS", "EPSG:4326");
        matchCountCommand.Parameters.AddWithValue("$destinationSRSnumber", int.Parse((await GetSRS(request.collectionID)).Split(":")[1]));
        matchCountCommand.Parameters.AddWithValue("$destinationSRS", await GetSRS(request.collectionID));

        var matchCountReader = matchCountCommand.ExecuteReader();
        matchCountReader.Read();
        var matches = matchCountReader.GetInt32(0);

        var commandText = "SELECT AsGPB(Transform(GeomFromGPB(geom), ($destinationSRSnumber), ST_GeomFromText(($linestringtext)), ($originSRS), ($destinationSRS))), * " + filterText + "LIMIT ($limit) OFFSET ($offset)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        //searchCommand.Parameters.AddWithValue("$table", request.collectionID);
        searchCommand.Parameters.AddWithValue("$linestringtext", $"LINESTRING({request.bbox.BottomLeft.X} {request.bbox.BottomLeft.Y}, {request.bbox.TopRight.X} {request.bbox.BottomLeft.Y}, {request.bbox.TopRight.X} {request.bbox.TopRight.Y}, {request.bbox.BottomLeft.X} {request.bbox.TopRight.Y}, {request.bbox.BottomLeft.X} {request.bbox.BottomLeft.Y})");
        searchCommand.Parameters.AddWithValue("$originSRS", await GetSRS(request.collectionID));
        searchCommand.Parameters.AddWithValue("$destinationSRSnumber", 4326);
        searchCommand.Parameters.AddWithValue("$destinationSRS", "EPSG:4326");
        searchCommand.Parameters.AddWithValue("$limit", request.limit);
        searchCommand.Parameters.AddWithValue("$offset", request.offset);
        var results = searchCommand.ExecuteReader();


        var features = await GetFeatures(results);

        var featureCollection = new ExtendedFeatureCollection()
        {
            Type = "FeatureCollection",
            Features = features,
            NumberMatched = matches,
            NumberReturned = features.Count,
            Links = new(),
        };

        return featureCollection;
    }

    public async Task<ExtendedFeature> GetFeature(string table, string id, string spatialReferenceSystem)
    {
        var originSRS = GetSRS(table);
        var geopackageWriter = new GeoPackageGeoWriter();
        var bboxBytes = geopackageWriter.Write(await GetBoundingBox(table));


        var commandText =
            "SELECT AsGPB(Transform(GeomFromGPB(geom), ($destinationSRSnumber), GeomFromGPB(($boundingBox)), ($originSRS), ($destinationSRS))), * " +
            $"FROM {table} WHERE id=($id)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("$table", table);
        searchCommand.Parameters.AddWithValue("$id", id);

        // The bounding box of the table, this makes the conversion more accurate.
        searchCommand.Parameters.AddWithValue("$boundingBox", bboxBytes);

        searchCommand.Parameters.AddWithValue("$originSRS", originSRS);
        searchCommand.Parameters.AddWithValue("$destinationSRSnumber", int.Parse(spatialReferenceSystem.Split(":")[1]));
        searchCommand.Parameters.AddWithValue("$destinationSRS", spatialReferenceSystem);

        var reader = searchCommand.ExecuteReader();

        var features = await GetFeatures(reader);

        return features.First();
    }

    private async Task<List<ExtendedFeature>> GetFeatures(SqliteDataReader reader)
    {
        var columns = reader.FieldCount;

        var geomColumn = 0;//reader.GetOrdinal("geom");

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
                Type = "Feature",
                ID = (string)properties["id"],
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = properties,
                Links = new(),
            });
        }

        return features;
    }

    private async Task<Geometry> GetBoundingBox(string table)
    {
        var srs = await GetSRS(table);

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

    private async Task<string> GetSRS(string table)
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

    internal async Task<IList<Collection>> GetCollectionsFromMetadata()
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
            collection.crs.Add("http://www.opengis.net/def/crs/OGC/1.3/CRS84");
            //collection.crs.Add("Whichever format it's originally in.");
            collection.Extent = new()
            {
                Spatial = new()
                {
                    BoundingBox = new()
                    {
                        BoundingBox.FromEnvelope((await GetBoundingBox(collection.ID)).EnvelopeInternal),
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
