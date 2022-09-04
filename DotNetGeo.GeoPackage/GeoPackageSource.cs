using DotNetGeo.Core;
using Microsoft.Data.Sqlite;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SQLitePCL;

namespace DotNetGeo.GeoPackage;

public class GeoPackageSource : IFeatureSource
{
    private SqliteConnection Connection { get; init; }

    public Collection Collection { get; init; }

    public GeoPackageSource(string dbFile)
    {
        var connectionString = new SqliteConnectionStringBuilder($"Data Source={dbFile}")
        {
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        Connection = new(connectionString);
    }

    public ExtendedFeatureCollection GetFeatures(SearchRequest request)
    {

        var matchCountCommandText =
            @"SELECT COUNT(*) FROM ($table)
            WHERE (($table).geom && ST_MakeEnvelope(($west), ($south), ($east), ($north), 4326))";
        var matchCountCommand = new SqliteCommand(matchCountCommandText, Connection);
        matchCountCommand.Parameters.AddWithValue("table", request.collectionID);
        matchCountCommand.Parameters.AddWithValue("limit", request.limit);
        matchCountCommand.Parameters.AddWithValue("offset", request.offset);
        matchCountCommand.Parameters.AddWithValue("west", request.bbox.BottomLeft.X);
        matchCountCommand.Parameters.AddWithValue("south", request.bbox.BottomLeft.Y);
        matchCountCommand.Parameters.AddWithValue("east", request.bbox.TopRight.X);
        matchCountCommand.Parameters.AddWithValue("north", request.bbox.TopRight.Y);

        var matches = matchCountCommand.ExecuteReader().GetInt32(0);

        var commandText =
            @"SELECT fid, id, geom FROM ($table)
            WHERE (($table).geom && ST_MakeEnvelope(($west), ($south), ($east), ($north), 4326))
            LIMIT ($limit) OFFSET ($offset)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("table", request.collectionID);
        searchCommand.Parameters.AddWithValue("limit", request.limit);
        searchCommand.Parameters.AddWithValue("offset", request.offset);
        searchCommand.Parameters.AddWithValue("west", request.bbox.BottomLeft.X);
        searchCommand.Parameters.AddWithValue("south", request.bbox.BottomLeft.Y);
        searchCommand.Parameters.AddWithValue("east", request.bbox.TopRight.X);
        searchCommand.Parameters.AddWithValue("north", request.bbox.TopRight.Y);
        var results = searchCommand.ExecuteReader();

        var geomColumn = results.GetOrdinal("geom");
        var idColumn = results.GetOrdinal("id");
        var fidColumn = results.GetOrdinal("fid");

        var features = new List<ExtendedFeature>();
        var geoPackageReader = new GeoPackageGeoReader();

        while (results.Read())
        {
            var id = results.GetString(idColumn);
            var fid = results.GetInt32(fidColumn);
            var geometryStream = results.GetStream(geomColumn);

            features.Add(new()
            {
                ID = id,
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = new()
                {
                    { "id", id },
                    { "fid", fid.ToString() },
                },
                Links = new(),
            });
        }

        var featureCollection = new ExtendedFeatureCollection()
        {
            Features = features,
            NumberMatched = matches,
            NumberReturned = features.Count,
            Links = new(),
        };

        return featureCollection;
    }

    public ExtendedFeature GetFeature(string id)
    {
        var commandText = @"SELECT fid, id, geom FROM ($table) WHERE id=($id)";
        var searchCommand = new SqliteCommand(commandText, Connection);
        searchCommand.Parameters.AddWithValue("table", "COLLECTIONID");
        searchCommand.Parameters.AddWithValue("id", id);
        var results = searchCommand.ExecuteReader();

        var geomColumn = results.GetOrdinal("geom");
        var fidColumn = results.GetOrdinal("fid");

        var features = new List<ExtendedFeature>();
        var geoPackageReader = new GeoPackageGeoReader();

        while (results.Read())
        {
            var fid = results.GetInt32(fidColumn);
            var geometryStream = results.GetStream(geomColumn);

            features.Add(new()
            {
                ID = id,
                Geometry = geoPackageReader.Read(geometryStream),
                Properties = new()
                {
                    { "id", id },
                    { "fid", fid.ToString() },
                },
                Links = new(),
            });
        }

        return features.First();
    }

    private void GetMetadata()
    {
        var commandText = @"SELECT * FROM gpkg_contents";
        var searchCommand = new SqliteCommand(commandText, Connection);

        var collections = new List<Collection>();
        var reader = searchCommand.ExecuteReader();

        var dataTypeColumn = reader.GetOrdinal("data_type");
        var titleColumn = reader.GetOrdinal("table_name");

        var minXColumn = reader.GetOrdinal("min_x");
        var minYColumn = reader.GetOrdinal("min_y");
        var maxXColumn = reader.GetOrdinal("max_x");
        var maxYColumn = reader.GetOrdinal("max_y");
        var srsColumn = reader.GetOrdinal("srs_id");

        while (reader.Read())
        {
            var dataType = reader.GetString(dataTypeColumn);
            if (!dataType.Equals("features")) continue;

            var id = reader.GetString(titleColumn);
            var title = id;
            var description = reader.GetString(titleColumn);

            // Note might not be in WGS84
            var minX = reader.GetString(minXColumn);
            var minY = reader.GetString(minYColumn);
            var maxX = reader.GetString(maxXColumn);
            var maxY = reader.GetString(maxYColumn);
            var srs = reader.GetString(srsColumn);

            collections.Add(new()
            {
                ID = id,
                Title = title,
                Description = description,
                Extent = null,
                Links = new(),
                crs = new(),
            });
        }
    }
}
